using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.Services.Auth;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Infrastructure.Data;
using DormitoryManagement.Infrastructure.Repositories;
using DormitoryManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Tests;

public sealed class SeedDataLoginTests
{
    [Fact]
    public async Task Seeded_roles_are_limited_to_admin_manager_and_student()
    {
        await using var dbContext = CreateContext();
        await dbContext.Database.EnsureCreatedAsync();

        var roles = await dbContext.Roles
            .OrderBy(role => role.Name)
            .Select(role => role.Name)
            .ToListAsync();

        Assert.Equal([RoleNames.Admin, RoleNames.Manager, RoleNames.Student], roles);
    }
    [Theory]
    [InlineData("admin", RoleNames.Admin)]
    [InlineData("admin@ktx.local", RoleNames.Admin)]
    [InlineData("manager", RoleNames.Manager)]
    [InlineData("manager@ktx.local", RoleNames.Manager)]
    [InlineData("building.manager", RoleNames.Manager)]
    [InlineData("building.manager@ktx.local", RoleNames.Manager)]
    [InlineData("staff", RoleNames.Manager)]
    [InlineData("staff@ktx.local", RoleNames.Manager)]
    [InlineData("student01", RoleNames.Student)]
    [InlineData("student01@ktx.local", RoleNames.Student)]
    [InlineData("SV001", RoleNames.Student)]
    public async Task Seeded_demo_accounts_login_with_demo_password(string login, string expectedRole)
    {
        await using var dbContext = CreateContext();
        await dbContext.Database.EnsureCreatedAsync();
        var session = new SessionService();
        var service = new AuthService(
            new UserRepository(dbContext),
            new PasswordHasherService(),
            session,
            session,
            new UnitOfWork(dbContext),
            new AuditLogService(dbContext, session),
            new FixedDateTimeProvider(),
            new SecurityOptions());

        var result = await service.LoginAsync(new LoginRequest
        {
            EmailOrUsernameOrStudentCode = login,
            Password = "123456"
        });

        Assert.True(result.Succeeded, result.ErrorMessage);
        Assert.Equal(expectedRole, result.User?.RoleName);
        Assert.Equal(expectedRole, session.CurrentUser?.RoleName);
    }

    private static DormitoryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DormitoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DormitoryDbContext(options);
    }

    private sealed class FixedDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = new(2026, 5, 14, 8, 0, 0, DateTimeKind.Utc);
    }
}

