using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.Infrastructure.Data;
using DormitoryManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Tests;

public sealed class DbInitializerTests
{
    [Fact]
    public async Task InitializeAsync_preserves_student01_pending_registration_on_relaunch()
    {
        await using var dbContext = CreateContext();
        var initializer = new DbInitializer(dbContext, new PasswordHasherService());

        await initializer.InitializeAsync();

        var student = await dbContext.Students.SingleAsync(x => x.StudentCode == "SV001");
        var room = await dbContext.Rooms.SingleAsync(x => x.RoomNumber == "104");

        dbContext.RoomRegistrations.Add(new RoomRegistration
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            RoomId = room.Id,
            Status = RegistrationStatus.Pending,
            ContractTermMonths = 6,
            IncludesInternet = true,
            RequestedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        await initializer.InitializeAsync();

        var registrations = await dbContext.RoomRegistrations
            .Where(x => x.StudentId == student.Id)
            .ToListAsync();

        var pendingRegistration = Assert.Single(registrations, x => x.Status == RegistrationStatus.Pending);
        Assert.Equal(room.Id, pendingRegistration.RoomId);
        Assert.DoesNotContain(registrations, x => x.Status == RegistrationStatus.Cancelled && x.RejectionReason == "Demo reset: student01 is available for registration.");
    }

    private static DormitoryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DormitoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new DormitoryDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }
}