using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Data;

internal static class SeedData
{
    private static readonly DateTime SeededAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime EffectiveFrom = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime InvoiceIssueDate = new(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime InvoiceDueDate = new(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime PaidAt = new(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc);

    private const string AdminPasswordHash = "PBKDF2-SHA256$100000$JCipfxfImHrCtND9jB6Jfw==$rw/DANHB2xRcV1nmDVz0ALdGNTdJAFx11ILhkdbNdE8=";
    private const string ManagerPasswordHash = "PBKDF2-SHA256$100000$PamHprATyGBC3ivsgWWdhQ==$rsaYYxIMFg9L6ijRV2JKZCS2ayIezOXhhzFkkOPDza0=";
    private const string BuildingManagerPasswordHash = "PBKDF2-SHA256$100000$shzcddxynvdmqHIpBAS/wQ==$CPeXdT7up4frQzlyddcXP9sNR7rml8supu2XIy/WdeM=";
    private const string StaffPasswordHash = "PBKDF2-SHA256$100000$x6lroz/EWVMoSbusCwB7ZQ==$CZwfTn5eHRl8Ph4KsM9Fm3H712ujYp647a+6LaR1NW8=";
    private const string StudentOnePasswordHash = "PBKDF2-SHA256$100000$4+/sphDQUiSqkjtRbc/jUw==$Zxdu2PLvBHNdNvG0pMRkdO60c54ePsyI0VmbXHaz5xc=";
    private const string StudentTwoPasswordHash = "PBKDF2-SHA256$100000$wqCHattCX2x6ywzPp3ZMJg==$MaXh00Fbg7v2ZT3FbkrSQui4HjcrSTK0wnhMh9DGoQM=";
    private const string StudentThreePasswordHash = "PBKDF2-SHA256$100000$hpZWt6wPlOB+vFm/FBP1pw==$XPfSDfwasR1DLPdvfPAo4Wpc7aYx5x8NP6dnPG7cloc=";

    private static class Roles
    {
        public static readonly Guid Admin = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public static readonly Guid Manager = Guid.Parse("10000000-0000-0000-0000-000000000002");
        public static readonly Guid Student = Guid.Parse("10000000-0000-0000-0000-000000000005");
    }

    private static class Users
    {
        public static readonly Guid Admin = Guid.Parse("20000000-0000-0000-0000-000000000001");
        public static readonly Guid Manager = Guid.Parse("20000000-0000-0000-0000-000000000002");
        public static readonly Guid BuildingManager = Guid.Parse("20000000-0000-0000-0000-000000000003");
        public static readonly Guid Staff = Guid.Parse("20000000-0000-0000-0000-000000000004");
        public static readonly Guid StudentOne = Guid.Parse("20000000-0000-0000-0000-000000000005");
        public static readonly Guid StudentTwo = Guid.Parse("20000000-0000-0000-0000-000000000006");
        public static readonly Guid StudentThree = Guid.Parse("20000000-0000-0000-0000-000000000007");
    }

    private static class Managers
    {
        public static readonly Guid Manager = Guid.Parse("30000000-0000-0000-0000-000000000001");
        public static readonly Guid BuildingManager = Guid.Parse("30000000-0000-0000-0000-000000000002");
        public static readonly Guid Staff = Guid.Parse("30000000-0000-0000-0000-000000000003");
    }

    private static class Buildings
    {
        public static readonly Guid A = Guid.Parse("40000000-0000-0000-0000-000000000001");
        public static readonly Guid B = Guid.Parse("40000000-0000-0000-0000-000000000002");
    }

    private static class Floors
    {
        public static readonly Guid A1 = Guid.Parse("41000000-0000-0000-0000-000000000001");
        public static readonly Guid A2 = Guid.Parse("41000000-0000-0000-0000-000000000002");
        public static readonly Guid B1 = Guid.Parse("41000000-0000-0000-0000-000000000003");
        public static readonly Guid B2 = Guid.Parse("41000000-0000-0000-0000-000000000004");
    }

    private static class Rooms
    {
        public static readonly Guid A101 = Guid.Parse("42000000-0000-0000-0000-000000000001");
        public static readonly Guid A102 = Guid.Parse("42000000-0000-0000-0000-000000000002");
        public static readonly Guid A103 = Guid.Parse("42000000-0000-0000-0000-000000000003");
        public static readonly Guid A201 = Guid.Parse("42000000-0000-0000-0000-000000000004");
        public static readonly Guid A202 = Guid.Parse("42000000-0000-0000-0000-000000000005");
        public static readonly Guid B101 = Guid.Parse("42000000-0000-0000-0000-000000000006");
        public static readonly Guid B102 = Guid.Parse("42000000-0000-0000-0000-000000000007");
        public static readonly Guid B103 = Guid.Parse("42000000-0000-0000-0000-000000000008");
        public static readonly Guid B201 = Guid.Parse("42000000-0000-0000-0000-000000000009");
        public static readonly Guid B202 = Guid.Parse("42000000-0000-0000-0000-000000000010");
    }

    private static class Students
    {
        public static readonly Guid One = Guid.Parse("50000000-0000-0000-0000-000000000001");
        public static readonly Guid Two = Guid.Parse("50000000-0000-0000-0000-000000000002");
        public static readonly Guid Three = Guid.Parse("50000000-0000-0000-0000-000000000003");
    }

    private static class FeeTypes
    {
        public static readonly Guid RoomFee = Guid.Parse("60000000-0000-0000-0000-000000000001");
        public static readonly Guid Electricity = Guid.Parse("60000000-0000-0000-0000-000000000002");
        public static readonly Guid Water = Guid.Parse("60000000-0000-0000-0000-000000000003");
        public static readonly Guid Internet = Guid.Parse("60000000-0000-0000-0000-000000000004");
        public static readonly Guid Parking = Guid.Parse("60000000-0000-0000-0000-000000000005");
        public static readonly Guid Deposit = Guid.Parse("60000000-0000-0000-0000-000000000006");
        public static readonly Guid Penalty = Guid.Parse("60000000-0000-0000-0000-000000000007");
    }

    private static class Invoices
    {
        public static readonly Guid One = Guid.Parse("70000000-0000-0000-0000-000000000001");
        public static readonly Guid Two = Guid.Parse("70000000-0000-0000-0000-000000000002");
        public static readonly Guid Three = Guid.Parse("70000000-0000-0000-0000-000000000003");
    }

    private static class Payments
    {
        public static readonly Guid One = Guid.Parse("80000000-0000-0000-0000-000000000001");
        public static readonly Guid Two = Guid.Parse("80000000-0000-0000-0000-000000000002");
    }

    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedRoles(modelBuilder);
        SeedUsers(modelBuilder);
        SeedBuildingsAndRooms(modelBuilder);
        SeedStudents(modelBuilder);
        SeedFees(modelBuilder);
        SeedInvoicesAndPayments(modelBuilder);
        SeedSupportTickets(modelBuilder);
    }

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = Roles.Admin, Name = RoleNames.Admin, Description = "System administrator", IsSystemRole = true, CreatedAt = SeededAt },
            new Role { Id = Roles.Manager, Name = RoleNames.Manager, Description = "Dormitory manager", IsSystemRole = true, CreatedAt = SeededAt },
            new Role { Id = Roles.Student, Name = RoleNames.Student, Description = "Student resident", IsSystemRole = true, CreatedAt = SeededAt });
    }

    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            User(Users.Admin, "admin", "admin@ktx.local", "System Admin", AdminPasswordHash, Roles.Admin),
            User(Users.Manager, "manager", "manager@ktx.local", "Dormitory Manager", ManagerPasswordHash, Roles.Manager),
            User(Users.BuildingManager, "building.manager", "building.manager@ktx.local", "Building Manager", BuildingManagerPasswordHash, Roles.Manager),
            User(Users.Staff, "staff", "staff@ktx.local", "Support Manager", StaffPasswordHash, Roles.Manager),
            User(Users.StudentOne, "student01", "student01@ktx.local", "Nguyen Van An", StudentOnePasswordHash, Roles.Student),
            User(Users.StudentTwo, "student02", "student02@ktx.local", "Tran Thi Binh", StudentTwoPasswordHash, Roles.Student),
            User(Users.StudentThree, "student03", "student03@ktx.local", "Le Minh Chau", StudentThreePasswordHash, Roles.Student));
    }

    private static User User(Guid id, string username, string email, string fullName, string passwordHash, Guid roleId) =>
        new()
        {
            Id = id,
            Username = username,
            Email = email,
            FullName = fullName,
            PasswordHash = passwordHash,
            Status = UserStatus.Active,
            RoleId = roleId,
            FailedLoginCount = 0,
            CreatedAt = SeededAt,
            IsDeleted = false
        };

    private static void SeedBuildingsAndRooms(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Building>().HasData(
            new Building { Id = Buildings.A, Code = "A", Name = "Building A", Address = "North campus", IsActive = true, CreatedAt = SeededAt, IsDeleted = false },
            new Building { Id = Buildings.B, Code = "B", Name = "Building B", Address = "South campus", IsActive = true, CreatedAt = SeededAt, IsDeleted = false });

        modelBuilder.Entity<Manager>().HasData(
            new Manager { Id = Managers.Manager, StaffCode = "MGR001", FullName = "Dormitory Manager", UserId = Users.Manager, IsBuildingManager = false, CreatedAt = SeededAt, IsDeleted = false },
            new Manager { Id = Managers.BuildingManager, StaffCode = "BM001", FullName = "Building Manager", UserId = Users.BuildingManager, BuildingId = Buildings.A, IsBuildingManager = true, CreatedAt = SeededAt, IsDeleted = false },
            new Manager { Id = Managers.Staff, StaffCode = "STF001", FullName = "Support Manager", UserId = Users.Staff, IsBuildingManager = false, CreatedAt = SeededAt, IsDeleted = false });

        modelBuilder.Entity<Floor>().HasData(
            new Floor { Id = Floors.A1, BuildingId = Buildings.A, FloorNumber = 1, Name = "A - Floor 1", CreatedAt = SeededAt, IsDeleted = false },
            new Floor { Id = Floors.A2, BuildingId = Buildings.A, FloorNumber = 2, Name = "A - Floor 2", CreatedAt = SeededAt, IsDeleted = false },
            new Floor { Id = Floors.B1, BuildingId = Buildings.B, FloorNumber = 1, Name = "B - Floor 1", CreatedAt = SeededAt, IsDeleted = false },
            new Floor { Id = Floors.B2, BuildingId = Buildings.B, FloorNumber = 2, Name = "B - Floor 2", CreatedAt = SeededAt, IsDeleted = false });

        modelBuilder.Entity<Room>().HasData(
            Room(Rooms.A101, Buildings.A, Floors.A1, "101", 4, 1, 750000m, RoomGenderType.Male),
            Room(Rooms.A102, Buildings.A, Floors.A1, "102", 4, 1, 750000m, RoomGenderType.Female),
            Room(Rooms.A103, Buildings.A, Floors.A1, "103", 4, 0, 750000m, RoomGenderType.Mixed),
            Room(Rooms.A201, Buildings.A, Floors.A2, "201", 6, 0, 650000m, RoomGenderType.Male),
            Room(Rooms.A202, Buildings.A, Floors.A2, "202", 6, 0, 650000m, RoomGenderType.Female),
            Room(Rooms.B101, Buildings.B, Floors.B1, "101", 4, 1, 700000m, RoomGenderType.Mixed),
            Room(Rooms.B102, Buildings.B, Floors.B1, "102", 4, 0, 700000m, RoomGenderType.Male),
            Room(Rooms.B103, Buildings.B, Floors.B1, "103", 4, 0, 700000m, RoomGenderType.Female),
            Room(Rooms.B201, Buildings.B, Floors.B2, "201", 8, 0, 600000m, RoomGenderType.Mixed),
            Room(Rooms.B202, Buildings.B, Floors.B2, "202", 8, 0, 600000m, RoomGenderType.Mixed));
    }

    private static Room Room(Guid id, Guid buildingId, Guid floorId, string roomNumber, int capacity, int occupancy, decimal monthlyPrice, RoomGenderType genderType) =>
        new()
        {
            Id = id,
            BuildingId = buildingId,
            FloorId = floorId,
            RoomNumber = roomNumber,
            Capacity = capacity,
            CurrentOccupancy = occupancy,
            MonthlyPrice = monthlyPrice,
            Status = occupancy >= capacity ? RoomStatus.Full : RoomStatus.Available,
            GenderType = genderType,
            CreatedAt = SeededAt,
            IsDeleted = false
        };

    private static void SeedStudents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>().HasData(
            new Student { Id = Students.One, StudentCode = "SV001", FullName = "Nguyen Van An", Email = "student01@ktx.local", PhoneNumber = "0900000001", Gender = "Male", Department = "Information Technology", ClassName = "IT01", UserId = Users.StudentOne, Status = StudentStatus.Staying, CreatedAt = SeededAt, IsDeleted = false },
            new Student { Id = Students.Two, StudentCode = "SV002", FullName = "Tran Thi Binh", Email = "student02@ktx.local", PhoneNumber = "0900000002", Gender = "Female", Department = "Business Administration", ClassName = "BA01", UserId = Users.StudentTwo, Status = StudentStatus.Staying, CreatedAt = SeededAt, IsDeleted = false },
            new Student { Id = Students.Three, StudentCode = "SV003", FullName = "Le Minh Chau", Email = "student03@ktx.local", PhoneNumber = "0900000003", Gender = "Other", Department = "Accounting", ClassName = "AC01", UserId = Users.StudentThree, Status = StudentStatus.Pending, CreatedAt = SeededAt, IsDeleted = false });
    }

    private static void SeedFees(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeeType>().HasData(
            FeeType(FeeTypes.RoomFee, "ROOM_FEE", "Room fee", "month", true),
            FeeType(FeeTypes.Electricity, "ELECTRICITY", "Electricity", "kWh", true),
            FeeType(FeeTypes.Water, "WATER", "Water", "m3", true),
            FeeType(FeeTypes.Internet, "INTERNET", "Internet", "month", true),
            FeeType(FeeTypes.Parking, "PARKING", "Parking", "month", true),
            FeeType(FeeTypes.Deposit, "DEPOSIT", "Deposit", "contract", false),
            FeeType(FeeTypes.Penalty, "PENALTY", "Penalty", "case", false));

        modelBuilder.Entity<FeeRate>().HasData(
            FeeRate(Guid.Parse("61000000-0000-0000-0000-000000000001"), FeeTypes.RoomFee, 750000m),
            FeeRate(Guid.Parse("61000000-0000-0000-0000-000000000002"), FeeTypes.Electricity, 3500m),
            FeeRate(Guid.Parse("61000000-0000-0000-0000-000000000003"), FeeTypes.Water, 19500m),
            FeeRate(Guid.Parse("61000000-0000-0000-0000-000000000004"), FeeTypes.Internet, 50000m),
            FeeRate(Guid.Parse("61000000-0000-0000-0000-000000000005"), FeeTypes.Parking, 100000m),
            FeeRate(Guid.Parse("61000000-0000-0000-0000-000000000006"), FeeTypes.Deposit, 1500000m),
            FeeRate(Guid.Parse("61000000-0000-0000-0000-000000000007"), FeeTypes.Penalty, 50000m));
    }

    private static FeeType FeeType(Guid id, string code, string name, string unit, bool isRecurring) =>
        new()
        {
            Id = id,
            Code = code,
            Name = name,
            Unit = unit,
            IsRecurring = isRecurring,
            CreatedAt = SeededAt,
            IsDeleted = false
        };

    private static FeeRate FeeRate(Guid id, Guid feeTypeId, decimal amount) =>
        new()
        {
            Id = id,
            FeeTypeId = feeTypeId,
            Amount = amount,
            EffectiveFrom = EffectiveFrom,
            CreatedAt = SeededAt
        };

    private static void SeedInvoicesAndPayments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>().HasData(
            Invoice(Invoices.One, "INV-2026-05-001", Students.One, Rooms.A101, "2026-05", 950000m, 950000m, InvoiceStatus.Paid),
            Invoice(Invoices.Two, "INV-2026-05-002", Students.Two, Rooms.A102, "2026-05", 900000m, 900000m, InvoiceStatus.Paid),
            Invoice(Invoices.Three, "INV-2026-05-003", Students.Three, Rooms.B101, "2026-05", 850000m, 0m, InvoiceStatus.Unpaid));

        modelBuilder.Entity<InvoiceItem>().HasData(
            InvoiceItem(Guid.Parse("71000000-0000-0000-0000-000000000001"), Invoices.One, FeeTypes.RoomFee, "Room fee May 2026", 1m, 750000m),
            InvoiceItem(Guid.Parse("71000000-0000-0000-0000-000000000002"), Invoices.One, FeeTypes.Electricity, "Electricity May 2026", 40m, 3500m),
            InvoiceItem(Guid.Parse("71000000-0000-0000-0000-000000000003"), Invoices.One, FeeTypes.Water, "Water May 2026", 5m, 12000m),
            InvoiceItem(Guid.Parse("71000000-0000-0000-0000-000000000004"), Invoices.Two, FeeTypes.RoomFee, "Room fee May 2026", 1m, 750000m),
            InvoiceItem(Guid.Parse("71000000-0000-0000-0000-000000000005"), Invoices.Two, FeeTypes.Internet, "Internet May 2026", 1m, 50000m),
            InvoiceItem(Guid.Parse("71000000-0000-0000-0000-000000000006"), Invoices.Two, FeeTypes.Water, "Water May 2026", 8.333333m, 12000m),
            InvoiceItem(Guid.Parse("71000000-0000-0000-0000-000000000007"), Invoices.Three, FeeTypes.RoomFee, "Room fee May 2026", 1m, 700000m),
            InvoiceItem(Guid.Parse("71000000-0000-0000-0000-000000000008"), Invoices.Three, FeeTypes.Internet, "Internet May 2026", 1m, 50000m),
            InvoiceItem(Guid.Parse("71000000-0000-0000-0000-000000000009"), Invoices.Three, FeeTypes.Parking, "Parking May 2026", 1m, 100000m));

        modelBuilder.Entity<Payment>().HasData(
            new Payment { Id = Payments.One, PaymentCode = "PAY-2026-05-001", StudentId = Students.One, InvoiceId = Invoices.One, Amount = 950000m, Method = PaymentMethod.QrBanking, Status = PaymentStatus.Success, TransactionRef = "QR-TXN-202605-001", PaidAt = PaidAt, CreatedAt = SeededAt },
            new Payment { Id = Payments.Two, PaymentCode = "PAY-2026-05-002", StudentId = Students.Two, InvoiceId = Invoices.Two, Amount = 900000m, Method = PaymentMethod.QrBanking, Status = PaymentStatus.Success, TransactionRef = "BANK-TXN-202605-002", PaidAt = PaidAt, CreatedAt = SeededAt });
    }

    private static Invoice Invoice(Guid id, string invoiceCode, Guid studentId, Guid roomId, string billingPeriod, decimal total, decimal paid, InvoiceStatus status) =>
        new()
        {
            Id = id,
            InvoiceNumber = invoiceCode,
            StudentId = studentId,
            RoomId = roomId,
            BillingPeriod = billingPeriod,
            InvoiceKind = InvoiceKind.MonthlyUtility,
            IssueDate = InvoiceIssueDate,
            DueDate = InvoiceDueDate,
            TotalAmount = total,
            PaidAmount = paid,
            Status = status,
            CreatedAt = SeededAt
        };

    private static InvoiceItem InvoiceItem(Guid id, Guid invoiceId, Guid feeTypeId, string description, decimal quantity, decimal unitPrice) =>
        new()
        {
            Id = id,
            InvoiceId = invoiceId,
            FeeTypeId = feeTypeId,
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Amount = Math.Round(quantity * unitPrice, 2),
            CreatedAt = SeededAt
        };

    private static void SeedSupportTickets(ModelBuilder modelBuilder)
    {
        var ticketOneId = Guid.Parse("90000000-0000-0000-0000-000000000001");
        var ticketTwoId = Guid.Parse("90000000-0000-0000-0000-000000000002");

        modelBuilder.Entity<SupportTicket>().HasData(
            new SupportTicket { Id = ticketOneId, StudentId = Students.One, CreatedByUserId = Users.StudentOne, AssignedToManagerId = Managers.Staff, Title = "Light bulb replacement", Description = "Room A101 needs a light bulb replacement.", Category = SupportTicketCategory.Maintenance, Status = SupportTicketStatus.InProgress, Priority = PriorityLevel.Medium, CreatedAt = SeededAt },
            new SupportTicket { Id = ticketTwoId, StudentId = Students.Two, CreatedByUserId = Users.StudentTwo, Title = "Invoice clarification", Description = "Please review the water charge for May.", Category = SupportTicketCategory.Billing, Status = SupportTicketStatus.New, Priority = PriorityLevel.Low, CreatedAt = SeededAt });

        modelBuilder.Entity<SupportTicketResponse>().HasData(
            new SupportTicketResponse { Id = Guid.Parse("91000000-0000-0000-0000-000000000001"), SupportTicketId = ticketOneId, UserId = Users.Staff, Message = "Maintenance staff has been assigned.", RespondedAt = SeededAt, CreatedAt = SeededAt });
    }

}


