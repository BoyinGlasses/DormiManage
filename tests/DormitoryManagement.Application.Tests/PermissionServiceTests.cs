using DormitoryManagement.Application.Security;
using DormitoryManagement.Domain.Constants;

namespace DormitoryManagement.Application.Tests;

public sealed class PermissionServiceTests
{
    [Theory]
    [InlineData(RoleNames.Manager)]
    public async Task HasPermissionAsync_allows_managers_to_read_students(string roleName)
    {
        var service = new PermissionService(new TestCurrentUser(roleName));

        Assert.True(await service.HasPermissionAsync(PermissionNames.StudentsRead));
    }

    [Fact]
    public async Task HasPermissionAsync_denies_unknown_permissions_for_students()
    {
        var service = new PermissionService(new TestCurrentUser(RoleNames.Student));

        Assert.False(await service.HasPermissionAsync("Forum.Legacy"));
    }
}

