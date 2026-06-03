using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Data;

public sealed class DbInitializer
{
    private readonly DormitoryDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private const string DemoPassword = "123456";

    public DbInitializer(DormitoryDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_dbContext.Database.IsRelational())
        {
            await _dbContext.Database.MigrateAsync(ct);
        }
        else
        {
            await _dbContext.Database.EnsureCreatedAsync(ct);
        }

        var adminRole = await EnsureRoleAsync(RoleNames.Admin, ct);
        var managerRole = await EnsureRoleAsync(RoleNames.Manager, ct);
        var buildingManagerRole = await EnsureRoleAsync(RoleNames.BuildingManager, ct);
        var staffRole = await EnsureRoleAsync(RoleNames.Staff, ct);
        var studentRole = await EnsureRoleAsync(RoleNames.Student, ct);

        await EnsureDemoUserAsync("admin", "admin@ktx.local", "System Admin", adminRole.Id, ct, "admin@dorm.local", "admin@example.local");
        var managerUser = await EnsureDemoUserAsync("manager", "manager@ktx.local", "Dormitory Manager", managerRole.Id, ct, "manager@dorm.local");
        var buildingManagerUser = await EnsureDemoUserAsync("building.manager", "building.manager@ktx.local", "Building Manager", buildingManagerRole.Id, ct, "building.manager@dorm.local");
        var staffUser = await EnsureDemoUserAsync("staff", "staff@ktx.local", "Support Staff", staffRole.Id, ct, "staff@dorm.local");
        var studentUser = await EnsureDemoUserAsync("student01", "student01@ktx.local", "Nguyen Van An", studentRole.Id, ct, "student001", "student001@dorm.local");
        await EnsureDemoUserAsync("student02", "student02@ktx.local", "Tran Thi Binh", studentRole.Id, ct);
        await EnsureDemoUserAsync("student03", "student03@ktx.local", "Le Minh Chau", studentRole.Id, ct);
        await EnsureStudentAsync(studentUser, ct);
        await EnsureDemoManagersAsync(managerUser, buildingManagerUser, staffUser, ct);
        await _dbContext.SaveChangesAsync(ct);
        await EnsureForumDemoDataAsync(ct);
        await _dbContext.SaveChangesAsync(ct);
        await EnsureDashboardDemoDataAsync(ct);

        await _dbContext.SaveChangesAsync(ct);
    }

    private async Task<Role> EnsureRoleAsync(string roleName, CancellationToken ct)
    {
        var role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Name == roleName, ct);
        if (role is not null)
        {
            return role;
        }

        role = new Role
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Roles.Add(role);
        return role;
    }

    private async Task<User> EnsureDemoUserAsync(
        string username,
        string email,
        string fullName,
        Guid roleId,
        CancellationToken ct,
        params string[] legacyKeys)
    {
        var normalizedKeys = legacyKeys
            .Append(username)
            .Append(email)
            .Select(x => x.Trim().ToLowerInvariant())
            .ToArray();

        var user = await _dbContext.Users.FirstOrDefaultAsync(
            x => normalizedKeys.Contains(x.Username.ToLower()) || normalizedKeys.Contains(x.Email.ToLower()),
            ct);

        var isNew = user is null;
        var legacyLookup = legacyKeys.Select(x => x.Trim().ToLowerInvariant()).ToArray();
        var isLegacyDemoUser = user is not null &&
            (legacyLookup.Contains(user.Username.ToLowerInvariant()) || legacyLookup.Contains(user.Email.ToLowerInvariant()));

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            _dbContext.Users.Add(user);
        }
        else
        {
            user.UpdatedAt = DateTime.UtcNow;
        }

        user.Username = username;
        user.Email = email;
        user.FullName = fullName;
        user.RoleId = roleId;
        if (isNew || isLegacyDemoUser)
        {
            user.Status = UserStatus.Active;
            user.FailedLoginCount = 0;
            user.LockedUntil = null;
            user.PasswordHash = _passwordHasher.HashPassword(DemoPassword);
        }

        return user;
    }

    private async Task EnsureStudentAsync(User studentUser, CancellationToken ct)
    {
        var student = await _dbContext.Students.FirstOrDefaultAsync(
            x => x.UserId == studentUser.Id || x.StudentCode == "SV001",
            ct);

        if (student is null)
        {
            student = new Student
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            _dbContext.Students.Add(student);
        }
        else
        {
            student.UpdatedAt = DateTime.UtcNow;
        }

        student.UserId = studentUser.Id;
        student.StudentCode = "SV001";
        student.FullName = studentUser.FullName;
        student.Email = studentUser.Email;
        student.Status = StudentStatus.Staying;
    }

    private async Task EnsureDemoManagersAsync(User managerUser, User buildingManagerUser, User staffUser, CancellationToken ct)
    {
        var buildingA = await _dbContext.Buildings.FirstOrDefaultAsync(x => x.Code == "A", ct);
        await EnsureManagerAsync("MGR001", managerUser, "Dormitory Manager", false, null, ct);
        await EnsureManagerAsync("BM001", buildingManagerUser, "Building Manager", true, buildingA?.Id, ct);
        await EnsureManagerAsync("STF001", staffUser, "Support Staff", false, null, ct);
    }

    private async Task EnsureManagerAsync(
        string staffCode,
        User user,
        string fullName,
        bool isBuildingManager,
        Guid? buildingId,
        CancellationToken ct)
    {
        var manager = await _dbContext.Managers.FirstOrDefaultAsync(
            x => x.StaffCode == staffCode || x.UserId == user.Id,
            ct);

        if (manager is null)
        {
            manager = new Manager
            {
                Id = Guid.NewGuid(),
                StaffCode = staffCode,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            _dbContext.Managers.Add(manager);
        }

        manager.UserId = user.Id;
        manager.FullName = fullName;
        manager.IsBuildingManager = isBuildingManager;
        manager.BuildingId = buildingId;
        manager.UpdatedAt = DateTime.UtcNow;
    }

    private async Task EnsureDashboardDemoDataAsync(CancellationToken ct)
    {
        var today = DateTime.Today;
        var period = today.ToString("yyyy-MM");
        var issueDate = new DateTime(today.Year, today.Month, 1);
        var admin = await _dbContext.Users.FirstAsync(x => x.Username == "admin", ct);
        var staff = await _dbContext.Managers.FirstOrDefaultAsync(x => x.StaffCode == "STF001", ct);
        var users = await _dbContext.Users.ToDictionaryAsync(x => x.Username, ct);
        await EnsureAdditionalDemoRoomsAsync(ct);
        var rooms = await _dbContext.Rooms
            .Include(x => x.Building)
            .ToDictionaryAsync(x => x.Building!.Code + "-" + x.RoomNumber, ct);

        ConfigureRoom(rooms, "A-101", 4, RoomStatus.Full);
        ConfigureRoom(rooms, "A-102", 3, RoomStatus.Available);
        ConfigureRoom(rooms, "A-103", 0, RoomStatus.Maintenance, RoomGenderType.Male);
        ConfigureRoom(rooms, "A-201", 3, RoomStatus.Available);
        ConfigureRoom(rooms, "A-202", 2, RoomStatus.Available);
        ConfigureRoom(rooms, "B-101", 2, RoomStatus.Available, RoomGenderType.Male);
        ConfigureRoom(rooms, "B-102", 1, RoomStatus.Available);
        ConfigureRoom(rooms, "B-103", 0, RoomStatus.Available);
        ConfigureRoom(rooms, "B-201", 2, RoomStatus.Available, RoomGenderType.Female);
        ConfigureRoom(rooms, "B-202", 0, RoomStatus.Available, RoomGenderType.Male);
        ConfigureRoom(rooms, "A-104", 0, RoomStatus.Available);
        ConfigureRoom(rooms, "A-105", 0, RoomStatus.Available);
        ConfigureRoom(rooms, "B-203", 0, RoomStatus.Available, RoomGenderType.Male);
        ConfigureRoom(rooms, "B-204", 0, RoomStatus.Available, RoomGenderType.Female);

        var demoStudents = new[]
        {
            new DemoStudent("SV001", "Nguyen Van An", "Male", "Information Technology", "IT01", StudentStatus.NotRegistered, null, "student01"),
            new DemoStudent("SV002", "Tran Thi Binh", "Female", "Business Administration", "BA01", StudentStatus.Staying, "A-102", "student02"),
            new DemoStudent("SV003", "Le Minh Chau", "Male", "Accounting", "AC01", StudentStatus.Pending, null, "student03"),
            new DemoStudent("SV004", "Pham Quoc Bao", "Male", "Information Technology", "IT02", StudentStatus.Staying, "A-101", null),
            new DemoStudent("SV005", "Do Hoang Long", "Male", "Civil Engineering", "CE01", StudentStatus.Staying, "A-101", null),
            new DemoStudent("SV006", "Hoang Minh Quan", "Male", "Mechanical Engineering", "ME01", StudentStatus.Staying, "A-101", null),
            new DemoStudent("SV007", "Vo Ngoc Mai", "Female", "Marketing", "MK01", StudentStatus.Staying, "A-102", null),
            new DemoStudent("SV008", "Bui Thanh Ha", "Female", "Finance", "FN01", StudentStatus.Staying, "A-102", null),
            new DemoStudent("SV009", "Dang Tuan Kiet", "Male", "Information Systems", "IS01", StudentStatus.Staying, "A-201", null),
            new DemoStudent("SV010", "Phan Duc Huy", "Male", "Architecture", "AR01", StudentStatus.Staying, "A-201", null),
            new DemoStudent("SV011", "Tran Gia Phuc", "Male", "Logistics", "LG01", StudentStatus.Staying, "A-201", null),
            new DemoStudent("SV012", "Nguyen Thu Trang", "Female", "English Studies", "EN01", StudentStatus.Staying, "A-202", null),
            new DemoStudent("SV013", "Le Khanh Linh", "Female", "Tourism", "TR01", StudentStatus.Staying, "A-202", null),
            new DemoStudent("SV014", "Vu Minh Tam", "Male", "Data Science", "DS01", StudentStatus.Staying, "B-101", null),
            new DemoStudent("SV015", "Mai Anh Khoa", "Male", "Cybersecurity", "CS01", StudentStatus.Staying, "B-101", null),
            new DemoStudent("SV016", "Pham Thanh Dat", "Male", "Electrical Engineering", "EE01", StudentStatus.Staying, "B-102", null),
            new DemoStudent("SV017", "Nguyen Bao Ngoc", "Female", "Human Resources", "HR01", StudentStatus.Staying, "B-201", null),
            new DemoStudent("SV018", "Do Minh Anh", "Female", "International Business", "IB01", StudentStatus.Staying, "B-201", null),
            new DemoStudent("SV019", "Tran Hoai Nam", "Male", "Software Engineering", "SE01", StudentStatus.Pending, null, null),
            new DemoStudent("SV020", "Le Thanh Ngan", "Female", "Banking", "BK01", StudentStatus.Pending, null, null)
        };

        var students = new Dictionary<string, Student>();
        foreach (var demo in demoStudents)
        {
            var student = await _dbContext.Students.FirstOrDefaultAsync(x => x.StudentCode == demo.Code, ct);
            if (student is null)
            {
                student = new Student
                {
                    Id = Guid.NewGuid(),
                    StudentCode = demo.Code,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                _dbContext.Students.Add(student);
            }

            student.FullName = demo.Name;
            student.Email = demo.UserName is not null && users.TryGetValue(demo.UserName, out var user)
                ? user.Email
                : demo.Code.ToLowerInvariant() + "@ktx.local";
            student.PhoneNumber = "09" + demo.Code[^3..].PadLeft(8, '0');
            student.Gender = demo.Gender;
            student.Department = demo.Department;
            student.ClassName = demo.ClassName;
            student.EnrollmentDate = new DateTime(2025, 9, 1);
            student.Status = demo.Status;
            student.UserId = demo.UserName is not null && users.TryGetValue(demo.UserName, out var mappedUser) ? mappedUser.Id : student.UserId;
            student.CurrentRoomId = demo.RoomKey is not null && rooms.TryGetValue(demo.RoomKey, out var room) ? room.Id : null;
            student.IsDeleted = false;
            student.UpdatedAt = DateTime.UtcNow;
            students[demo.Code] = student;
        }

        foreach (var demo in demoStudents.Where(x => x.RoomKey is not null))
        {
            var student = students[demo.Code];
            var room = rooms[demo.RoomKey!];
            var assignment = await _dbContext.RoomAssignments.FirstOrDefaultAsync(x => x.StudentId == student.Id && x.IsActive, ct);
            if (assignment is null)
            {
                assignment = new RoomAssignment
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _dbContext.RoomAssignments.Add(assignment);
            }

            assignment.RoomId = room.Id;
            assignment.StartDate = new DateTime(2026, 1, 15);
            assignment.EndDate = null;
            assignment.IsActive = true;

            await EnsureContractAsync(student, room, assignment.StartDate, ct);
        }

        foreach (var demo in demoStudents.Where(x => x.RoomKey is null))
        {
            var student = students[demo.Code];
            var assignments = await _dbContext.RoomAssignments
                .Where(x => x.StudentId == student.Id && x.IsActive)
                .ToListAsync(ct);

            foreach (var assignment in assignments)
            {
                assignment.IsActive = false;
                assignment.EndDate ??= today.AddDays(-1);
                assignment.UpdatedAt = DateTime.UtcNow;
            }
        }

        await EnsureRegistrationAsync(students["SV003"], rooms["B-101"], RegistrationStatus.Pending, today.AddDays(-2), null, null, admin.Id, ct);
        await EnsureRegistrationAsync(students["SV019"], rooms["A-202"], RegistrationStatus.Pending, today.AddDays(-1), null, null, admin.Id, ct);
        await EnsureRegistrationAsync(students["SV020"], rooms["B-201"], RegistrationStatus.Pending, today.AddHours(-8), null, null, admin.Id, ct);
        await EnsureRegistrationAsync(students["SV002"], rooms["A-102"], RegistrationStatus.Approved, today.AddDays(-32), today.AddDays(-31), null, admin.Id, ct);
        await EnsureRegistrationAsync(students["SV018"], rooms["B-201"], RegistrationStatus.Rejected, today.AddDays(-10), today.AddDays(-9), "Preferred room was full.", admin.Id, ct);
        await RepairApprovedRegistrationsAsync(admin.Id, ct);
        await _dbContext.SaveChangesAsync(ct);
        await RecalculateRoomOccupancyAsync(ct);

        var invoices = new[]
        {
            new DemoInvoice($"INV-{period}-001", "SV001", "A-101", 950000m, 950000m, InvoiceStatus.Paid, today.AddDays(6)),
            new DemoInvoice($"INV-{period}-002", "SV002", "A-102", 900000m, 500000m, InvoiceStatus.Partial, today.AddDays(-4)),
            new DemoInvoice($"INV-{period}-003", "SV003", "B-101", 850000m, 0m, InvoiceStatus.Unpaid, today.AddDays(-6)),
            new DemoInvoice($"INV-{period}-004", "SV004", "A-101", 920000m, 0m, InvoiceStatus.Overdue, today.AddDays(-7)),
            new DemoInvoice($"INV-{period}-005", "SV007", "A-102", 880000m, 880000m, InvoiceStatus.Paid, today.AddDays(4)),
            new DemoInvoice($"INV-{period}-006", "SV010", "A-201", 720000m, 300000m, InvoiceStatus.Partial, today.AddDays(8)),
            new DemoInvoice($"INV-{period}-007", "SV015", "B-101", 810000m, 0m, InvoiceStatus.Unpaid, today.AddDays(10)),
            new DemoInvoice($"INV-{period}-008", "SV018", "B-201", 700000m, 700000m, InvoiceStatus.Paid, today.AddDays(7)),
            new DemoInvoice($"INV-{period}-009", "SV001", "B-101", 780000m, 0m, InvoiceStatus.Unpaid, today.AddDays(9))
        };

        foreach (var demo in invoices)
        {
            var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(x => x.InvoiceNumber == demo.Number, ct);
            if (invoice is null)
            {
                invoice = new Invoice { Id = Guid.NewGuid(), InvoiceNumber = demo.Number, CreatedAt = DateTime.UtcNow };
                _dbContext.Invoices.Add(invoice);
            }

            invoice.StudentId = students[demo.StudentCode].Id;
            invoice.RoomId = rooms[demo.RoomKey].Id;
            invoice.BillingPeriod = period;
            invoice.IssueDate = issueDate;
            invoice.DueDate = demo.DueDate;
            invoice.TotalAmount = demo.Total;
            invoice.PaidAmount = demo.Paid;
            invoice.Status = demo.Status;
        }

        await EnsurePaymentAsync($"PAY-{period}-001", students["SV001"], 950000m, PaymentMethod.MockGateway, today.AddDays(-3), ct);
        await EnsurePaymentAsync($"PAY-{period}-002", students["SV002"], 500000m, PaymentMethod.BankTransfer, today.AddDays(-2), ct);
        await EnsurePaymentAsync($"PAY-{period}-003", students["SV007"], 880000m, PaymentMethod.Cash, today.AddDays(-1), ct);
        await EnsurePaymentAsync($"PAY-{period}-004", students["SV018"], 700000m, PaymentMethod.MockGateway, today.AddHours(-6), ct);
        await EnsurePendingPaymentAsync($"PAY-{period}-P01", students["SV002"], 200000m, PaymentMethod.BankTransfer, today.AddHours(-10), ct);
        await EnsurePendingPaymentAsync($"PAY-{period}-P02", students["SV003"], 300000m, PaymentMethod.MockGateway, today.AddHours(-4), ct);
        await EnsurePendingPaymentAsync($"PAY-{period}-P03", students["SV015"], 250000m, PaymentMethod.BankTransfer, today.AddHours(-3), ct);
        await EnsurePendingPaymentAsync($"PAY-{period}-P04", students["SV010"], 200000m, PaymentMethod.MockGateway, today.AddHours(-2), ct);

        await EnsureTicketAsync("Light bulb replacement", students["SV001"], users.GetValueOrDefault("student01") ?? admin, staff, "Room A101 needs a light bulb replacement.", SupportTicketCategory.Maintenance, SupportTicketStatus.InProgress, PriorityLevel.Medium, DateTime.UtcNow.AddHours(-18), ct);
        await EnsureTicketAsync("Invoice clarification", students["SV002"], users.GetValueOrDefault("student02") ?? admin, null, "Please review the water charge for May.", SupportTicketCategory.Billing, SupportTicketStatus.New, PriorityLevel.Low, DateTime.UtcNow.AddHours(-12), ct);
        await EnsureTicketAsync("Air conditioner noise", students["SV004"], users.GetValueOrDefault("student01") ?? admin, staff, "A101 air conditioner has been noisy at night.", SupportTicketCategory.Maintenance, SupportTicketStatus.New, PriorityLevel.High, DateTime.UtcNow.AddHours(-7), ct);
        await EnsureTicketAsync("Wifi unstable in B building", students["SV015"], users.GetValueOrDefault("student02") ?? admin, staff, "Wireless signal drops frequently near B101.", SupportTicketCategory.Other, SupportTicketStatus.InProgress, PriorityLevel.Medium, DateTime.UtcNow.AddHours(-5), ct);
        await EnsureTicketAsync("Lost access card", students["SV019"], users.GetValueOrDefault("student03") ?? admin, null, "Student needs a replacement access card.", SupportTicketCategory.Other, SupportTicketStatus.New, PriorityLevel.Urgent, DateTime.UtcNow.AddHours(-2), ct);
        await EnsureAuditLogAsync(admin.Id, "Login.Success", "User", admin.Id, "System Admin signed in.", DateTime.UtcNow.AddHours(-4), ct);
        await EnsureAuditLogAsync(admin.Id, "RoomRegistration.Submitted", "RoomRegistration", null, "Le Minh Chau requested B-101.", DateTime.UtcNow.AddHours(-3), ct);
        await EnsureAuditLogAsync(admin.Id, "Invoice.Generated", "Invoice", null, "Demo monthly invoices generated.", DateTime.UtcNow.AddHours(-2), ct);
        await EnsureAuditLogAsync(admin.Id, "Payment.Created", "Payment", null, "Pending payment received for review.", DateTime.UtcNow.AddHours(-1), ct);
        await EnsureNotificationAsync(students["SV001"].UserId, "Registration approved", "Your room registration has been approved.", false, DateTime.UtcNow.AddHours(-3), ct);
        await EnsureNotificationAsync(students["SV002"].UserId, "Payment pending", "Your payment is waiting for admin confirmation.", false, DateTime.UtcNow.AddHours(-2), ct);
        await EnsureNotificationAsync(users.GetValueOrDefault("manager")?.Id, "New pending registration", "A student submitted a room registration.", false, DateTime.UtcNow.AddHours(-1), ct);
    }

    private async Task EnsureForumDemoDataAsync(CancellationToken ct)
    {
        var users = await _dbContext.Users.ToDictionaryAsync(x => x.Username, ct);
        var admin = users.GetValueOrDefault("admin") ?? users.Values.First();
        var manager = users.GetValueOrDefault("manager") ?? admin;
        var student = users.GetValueOrDefault("student01") ?? admin;
        var now = DateTime.UtcNow;

        await EnsureForumPostAsync(
            title: "Maintenance schedule for electricity and water in Building B",
            author: manager,
            category: "Announcements",
            area: "Building B",
            content: "Building B will have a scheduled electricity and water maintenance window this Saturday from 08:00 to 11:30. Please charge essential devices in advance and prepare drinking water for the morning.",
            tags: ["maintenance", "building-b", "announcement"],
            createdAt: now.AddHours(-2),
            viewCount: 245,
            isPinned: true,
            isImportant: true,
            ct);

        await EnsureForumPostAsync(
            title: "Volunteer club recruitment",
            author: student,
            category: "Events",
            area: "Study Hall",
            content: "The volunteer club is recruiting students for weekend community activities. Interested residents can join the orientation session in the study hall this Friday evening.",
            tags: ["clubs", "volunteer", "events"],
            createdAt: now.AddHours(-8),
            viewCount: 87,
            isPinned: false,
            isImportant: false,
            ct);

        await EnsureForumPostAsync(
            title: "New cafeteria food review",
            author: student,
            category: "General",
            area: "Cafeteria",
            content: "The cafeteria added several lunch options this week. The rice bowl is quick and affordable, while the soup counter is best before peak hour.",
            tags: ["cafeteria", "review", "food"],
            createdAt: now.AddDays(-1),
            viewCount: 132,
            isPinned: false,
            isImportant: false,
            ct);

        await EnsureForumPostAsync(
            title: "Emergency after-hours contact guide",
            author: admin,
            category: "Support",
            area: "Front Desk",
            content: "For after-hours emergencies, call the front desk hotline first. Security can support urgent building access, maintenance leaks, and safety concerns until office hours resume.",
            tags: ["support", "emergency", "front-desk"],
            createdAt: now.AddDays(-2),
            viewCount: 301,
            isPinned: true,
            isImportant: true,
            ct);

        await EnsureForumPostAsync(
            title: "Wi-Fi setup guide for Buildings A and C",
            author: manager,
            category: "Guides",
            area: "Buildings A and C",
            content: "Students in Buildings A and C should forget the old network, reconnect to the dormitory SSID, and sign in with their student code. Contact support if the device cannot authenticate.",
            tags: ["wifi", "guide", "building-a", "building-c"],
            createdAt: now.AddDays(-3),
            viewCount: 176,
            isPinned: false,
            isImportant: false,
            ct);
    }

    private async Task EnsureForumPostAsync(
        string title,
        User author,
        string category,
        string? area,
        string content,
        IReadOnlyCollection<string> tags,
        DateTime createdAt,
        int viewCount,
        bool isPinned,
        bool isImportant,
        CancellationToken ct)
    {
        var post = await _dbContext.ForumPosts
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Title == title, ct);

        if (post is null)
        {
            post = new ForumPost
            {
                Id = Guid.NewGuid(),
                Title = title,
                CreatedAt = createdAt,
                Status = ForumPostStatus.Published
            };
            _dbContext.ForumPosts.Add(post);
        }

        post.AuthorUserId = author.Id;
        post.Content = content;
        post.Excerpt = content.Length <= 240 ? content : content[..237] + "...";
        post.Category = category;
        post.Area = area;
        post.ViewCount = viewCount;
        post.IsPinned = isPinned;
        post.IsImportant = isImportant;
        post.Status = ForumPostStatus.Published;
        post.UpdatedAt = DateTime.UtcNow;

        var normalizedTags = tags.Select(x => x.Trim().TrimStart('#').ToLowerInvariant()).Where(x => x.Length > 0).Distinct().ToArray();
        foreach (var tag in post.Tags.Where(tag => !normalizedTags.Contains(tag.Name)).ToArray())
        {
            post.Tags.Remove(tag);
        }

        var existingTags = post.Tags.Select(tag => tag.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var tag in normalizedTags.Where(tag => !existingTags.Contains(tag)))
        {
            post.Tags.Add(new ForumPostTag
            {
                Id = Guid.NewGuid(),
                ForumPostId = post.Id,
                Name = tag
            });
        }
    }

    private async Task EnsureAdditionalDemoRoomsAsync(CancellationToken ct)
    {
        await EnsureDemoRoomAsync("A", 1, "104", 4, 750000m, RoomGenderType.Male, ct);
        await EnsureDemoRoomAsync("A", 1, "105", 4, 750000m, RoomGenderType.Female, ct);
        await EnsureDemoRoomAsync("B", 2, "203", 8, 600000m, RoomGenderType.Male, ct);
        await EnsureDemoRoomAsync("B", 2, "204", 8, 600000m, RoomGenderType.Female, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    private async Task EnsureDemoRoomAsync(string buildingCode, int floorNumber, string roomNumber, int capacity, decimal monthlyPrice, RoomGenderType genderType, CancellationToken ct)
    {
        var building = await _dbContext.Buildings.FirstAsync(x => x.Code == buildingCode, ct);
        var floor = await _dbContext.Floors.FirstAsync(x => x.BuildingId == building.Id && x.FloorNumber == floorNumber, ct);
        var room = await _dbContext.Rooms.FirstOrDefaultAsync(x => x.BuildingId == building.Id && x.FloorId == floor.Id && x.RoomNumber == roomNumber, ct);
        if (room is null)
        {
            room = new Room
            {
                Id = Guid.NewGuid(),
                BuildingId = building.Id,
                FloorId = floor.Id,
                RoomNumber = roomNumber,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            _dbContext.Rooms.Add(room);
        }

        room.Capacity = capacity;
        room.CurrentOccupancy = 0;
        room.MonthlyPrice = monthlyPrice;
        room.Status = RoomStatus.Available;
        room.GenderType = genderType;
        room.IsDeleted = false;
        room.UpdatedAt = DateTime.UtcNow;
    }

    private static void ConfigureRoom(IReadOnlyDictionary<string, Room> rooms, string key, int occupancy, RoomStatus status, RoomGenderType? genderType = null)
    {
        if (!rooms.TryGetValue(key, out var room))
        {
            return;
        }

        room.CurrentOccupancy = occupancy;
        room.Status = status;
        if (genderType.HasValue)
        {
            room.GenderType = genderType.Value;
        }

        room.UpdatedAt = DateTime.UtcNow;
    }


    private async Task EnsureRegistrationAsync(
        Student student,
        Room room,
        RegistrationStatus status,
        DateTime requestedAt,
        DateTime? reviewedAt,
        string? rejectionReason,
        Guid reviewedByUserId,
        CancellationToken ct)
    {
        var registration = await _dbContext.RoomRegistrations.FirstOrDefaultAsync(
            x => x.StudentId == student.Id && x.RoomId == room.Id && x.Status == status,
            ct);

        if (registration is null)
        {
            registration = new RoomRegistration
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                RoomId = room.Id,
                CreatedAt = requestedAt
            };
            _dbContext.RoomRegistrations.Add(registration);
        }

        registration.Status = status;
        registration.RequestedAt = requestedAt;
        registration.ReviewedAt = reviewedAt;
        registration.ReviewedByUserId = reviewedAt.HasValue ? reviewedByUserId : null;
        registration.RejectionReason = rejectionReason;
    }

    private async Task RepairApprovedRegistrationsAsync(Guid reviewedByUserId, CancellationToken ct)
    {
        var approvedRegistrations = await _dbContext.RoomRegistrations
            .Include(x => x.Student)
            .Include(x => x.Room)
            .Where(x => x.Status == RegistrationStatus.Approved)
            .OrderByDescending(x => x.ReviewedAt ?? x.RequestedAt)
            .ToListAsync(ct);

        foreach (var group in approvedRegistrations.GroupBy(x => x.StudentId))
        {
            var registration = group.First();
            if (registration.Student is null || registration.Room is null)
            {
                continue;
            }

            var staleRegistrations = await _dbContext.RoomRegistrations
                .Where(x => x.StudentId == registration.StudentId
                    && x.Id != registration.Id
                    && (x.Status == RegistrationStatus.Pending || x.Status == RegistrationStatus.Approved))
                .ToListAsync(ct);
            foreach (var staleApproved in staleRegistrations)
            {
                staleApproved.Status = RegistrationStatus.Cancelled;
                staleApproved.RejectionReason = "Automatically cancelled after another room registration was approved.";
                staleApproved.ReviewedAt ??= DateTime.UtcNow;
                staleApproved.ReviewedByUserId ??= reviewedByUserId;
                staleApproved.UpdatedAt = DateTime.UtcNow;
            }

            var startDate = (registration.ReviewedAt ?? registration.RequestedAt).Date;
            if (startDate == default)
            {
                startDate = DateTime.Today;
            }

            var assignments = await _dbContext.RoomAssignments
                .Where(x => x.StudentId == registration.StudentId)
                .ToListAsync(ct);
            var currentAssignment = assignments.FirstOrDefault(x => x.RoomId == registration.RoomId);
            foreach (var assignment in assignments.Where(x => x.Id != currentAssignment?.Id))
            {
                assignment.IsActive = false;
                assignment.EndDate ??= DateTime.Today.AddDays(-1);
                assignment.UpdatedAt = DateTime.UtcNow;
            }

            if (currentAssignment is null)
            {
                currentAssignment = new RoomAssignment
                {
                    Id = Guid.NewGuid(),
                    StudentId = registration.StudentId,
                    RoomId = registration.RoomId,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.RoomAssignments.Add(currentAssignment);
            }

            currentAssignment.StartDate = startDate;
            currentAssignment.EndDate = null;
            currentAssignment.IsActive = true;
            currentAssignment.UpdatedAt = DateTime.UtcNow;

            var activeContracts = await _dbContext.Contracts
                .Where(x => x.StudentId == registration.StudentId && x.Status == ContractStatus.Active && x.RoomId != registration.RoomId)
                .ToListAsync(ct);
            foreach (var contract in activeContracts)
            {
                contract.Status = ContractStatus.Terminated;
                var terminatedAt = DateTime.Today.AddDays(-1);
                contract.EndDate = terminatedAt < contract.StartDate ? contract.StartDate : terminatedAt;
                contract.UpdatedAt = DateTime.UtcNow;
            }

            await EnsureContractAsync(registration.Student, registration.Room, startDate, ct);
            registration.Student.CurrentRoomId = registration.RoomId;
            registration.Student.Status = StudentStatus.Staying;
            registration.Student.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task RecalculateRoomOccupancyAsync(CancellationToken ct)
    {
        var activeCounts = await _dbContext.RoomAssignments
            .Where(x => x.IsActive)
            .GroupBy(x => x.RoomId)
            .Select(x => new { RoomId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.RoomId, x => x.Count, ct);
        var rooms = await _dbContext.Rooms.Where(x => !x.IsDeleted).ToListAsync(ct);

        foreach (var room in rooms)
        {
            activeCounts.TryGetValue(room.Id, out var activeCount);
            room.CurrentOccupancy = activeCount;
            if (room.Status is RoomStatus.Available or RoomStatus.Full)
            {
                room.Status = activeCount >= room.Capacity ? RoomStatus.Full : RoomStatus.Available;
            }

            room.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task EnsureContractAsync(Student student, Room room, DateTime startDate, CancellationToken ct)
    {
        var contract = await _dbContext.Contracts.FirstOrDefaultAsync(
            x => x.StudentId == student.Id && x.RoomId == room.Id && x.Status == ContractStatus.Active,
            ct);

        if (contract is null)
        {
            contract = new Contract
            {
                Id = Guid.NewGuid(),
                ContractNumber = $"CTR-DEMO-{student.StudentCode}-{room.RoomNumber}",
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Contracts.Add(contract);
        }

        contract.StudentId = student.Id;
        contract.RoomId = room.Id;
        contract.StartDate = startDate.Date;
        contract.EndDate = startDate.Date.AddMonths(12).AddDays(-1);
        contract.MonthlyFee = room.MonthlyPrice;
        contract.DepositAmount = 0m;
        contract.Status = ContractStatus.Active;
        contract.UpdatedAt = DateTime.UtcNow;
    }

    private async Task EnsurePaymentAsync(string code, Student student, decimal amount, PaymentMethod method, DateTime paidAt, CancellationToken ct)
    {
        var payment = await _dbContext.Payments.FirstOrDefaultAsync(x => x.PaymentCode == code, ct);
        if (payment is null)
        {
            payment = new Payment { Id = Guid.NewGuid(), PaymentCode = code, CreatedAt = paidAt };
            _dbContext.Payments.Add(payment);
        }

        payment.StudentId = student.Id;
        payment.Amount = amount;
        payment.Method = method;
        payment.Status = PaymentStatus.Success;
        payment.TransactionRef = "DEMO-" + code;
        payment.PaidAt = paidAt;
    }

    private async Task EnsurePendingPaymentAsync(string code, Student student, decimal amount, PaymentMethod method, DateTime createdAt, CancellationToken ct)
    {
        var payment = await _dbContext.Payments.FirstOrDefaultAsync(x => x.PaymentCode == code, ct);
        if (payment is null)
        {
            payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentCode = code,
                CreatedAt = createdAt,
                Status = PaymentStatus.Pending
            };
            _dbContext.Payments.Add(payment);
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return;
        }

        payment.StudentId = student.Id;
        payment.Amount = amount;
        payment.Method = method;
        payment.TransactionRef = null;
        payment.PaidAt = null;
    }

    private async Task EnsureTicketAsync(
        string title,
        Student student,
        User createdBy,
        Manager? assignedTo,
        string description,
        SupportTicketCategory category,
        SupportTicketStatus status,
        PriorityLevel priority,
        DateTime createdAt,
        CancellationToken ct)
    {
        var ticket = await _dbContext.SupportTickets.FirstOrDefaultAsync(x => x.Title == title, ct);
        if (ticket is null)
        {
            ticket = new SupportTicket { Id = Guid.NewGuid(), Title = title };
            _dbContext.SupportTickets.Add(ticket);
        }

        ticket.StudentId = student.Id;
        ticket.CreatedByUserId = createdBy.Id;
        ticket.AssignedToManagerId = assignedTo?.Id;
        ticket.Description = description;
        ticket.Category = category;
        ticket.Status = status;
        ticket.Priority = priority;
        ticket.CreatedAt = createdAt;
    }

    private async Task EnsureAuditLogAsync(
        Guid? userId,
        string action,
        string entityName,
        Guid? entityId,
        string details,
        DateTime createdAt,
        CancellationToken ct)
    {
        var exists = await _dbContext.AuditLogs.AnyAsync(
            x => x.Action == action && x.EntityName == entityName && x.Details == details,
            ct);
        if (exists)
        {
            return;
        }

        _dbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            Workstation = Environment.MachineName,
            CreatedAt = createdAt
        });
    }

    private async Task EnsureNotificationAsync(
        Guid? userId,
        string title,
        string message,
        bool isRead,
        DateTime createdAt,
        CancellationToken ct)
    {
        if (!userId.HasValue)
        {
            return;
        }

        var existing = await _dbContext.Notifications
            .Include(x => x.UserNotifications)
            .FirstOrDefaultAsync(x => x.Title == title && x.Message == message, ct);

        if (existing is null)
        {
            existing = new Notification
            {
                Id = Guid.NewGuid(),
                Title = title,
                Message = message,
                SentAt = createdAt,
                CreatedAt = createdAt
            };
            _dbContext.Notifications.Add(existing);
        }

        if (existing.UserNotifications.Any(x => x.UserId == userId.Value))
        {
            return;
        }

        existing.UserNotifications.Add(new UserNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            IsRead = isRead,
            ReadAt = isRead ? createdAt : null
        });
    }

    private sealed record DemoStudent(
        string Code,
        string Name,
        string Gender,
        string Department,
        string ClassName,
        StudentStatus Status,
        string? RoomKey,
        string? UserName);

    private sealed record DemoInvoice(
        string Number,
        string StudentCode,
        string RoomKey,
        decimal Total,
        decimal Paid,
        InvoiceStatus Status,
        DateTime DueDate);

}




