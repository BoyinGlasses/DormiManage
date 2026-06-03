using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Forum;
using DormitoryManagement.Application.Security;
using DormitoryManagement.Application.Services.Forum;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Tests;

public sealed class ForumServiceTests
{
    [Fact]
    public async Task CreatePost_LinksAuthorToStudentProfile()
    {
        var fixture = ForumFixture.CreateStudent();
        var service = fixture.CreateService();

        var post = await service.CreatePostAsync(new CreateForumPostRequest
        {
            Title = "Room checklist",
            Content = "Remember to check lights before move-in.",
            CategoryId = fixture.CategoryId,
            TagIds = new[] { fixture.TagId }
        });

        Assert.Equal(fixture.CurrentUserId, post.Author.UserId);
        Assert.Equal("SV001", post.Author.StudentCode);
        Assert.Equal("Nguyen Van An", post.Author.DisplayName);
        Assert.Equal(RoleNames.Student, post.Author.RoleName);
    }

    [Fact]
    public async Task UpdatePost_RejectsNonAuthorStudent()
    {
        var fixture = ForumFixture.CreateStudent();
        var otherUserId = Guid.NewGuid();
        fixture.AddUser(otherUserId, RoleNames.Student, "Other Student");
        var post = fixture.AddPost(otherUserId, ForumVisibilityScope.Campus);
        var service = fixture.CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdatePostAsync(new UpdateForumPostRequest
        {
            PostId = post.Id,
            Title = "Edited",
            Content = "Trying to edit someone else's post.",
            CategoryId = fixture.CategoryId
        }));
    }

    [Fact]
    public async Task DeletePost_AllowsManagerToModerateOtherAuthors()
    {
        var fixture = ForumFixture.CreateManager();
        var studentUserId = Guid.NewGuid();
        fixture.AddUser(studentUserId, RoleNames.Student, "Student Author");
        var post = fixture.AddPost(studentUserId, ForumVisibilityScope.Campus);
        var service = fixture.CreateService();

        await service.DeletePostAsync(post.Id);

        Assert.True(post.IsDeleted);
        Assert.Equal(ForumPostStatus.Hidden, post.Status);
    }

    [Fact]
    public async Task GetPosts_BuildingManagerSeesAssignedBuildingNotOtherBuilding()
    {
        var buildingA = Guid.NewGuid();
        var buildingB = Guid.NewGuid();
        var fixture = ForumFixture.CreateBuildingManager(buildingA);
        fixture.UnitOfWork.Set<Building>().Items.Add(new Building { Id = buildingA, Code = "A", Name = "Building A" });
        fixture.UnitOfWork.Set<Building>().Items.Add(new Building { Id = buildingB, Code = "B", Name = "Building B" });
        fixture.AddPost(Guid.NewGuid(), ForumVisibilityScope.Building, buildingA);
        var hiddenPost = fixture.AddPost(Guid.NewGuid(), ForumVisibilityScope.Building, buildingB);
        var service = fixture.CreateService();

        var result = await service.GetPostsAsync();

        Assert.DoesNotContain(result.Items, post => post.Id == hiddenPost.Id);
        Assert.All(result.Items.Where(post => post.VisibilityScope == ForumVisibilityScope.Building), post => Assert.Equal(buildingA, post.BuildingId));
    }

    [Fact]
    public async Task CreatePost_RejectsInactiveCategoryOrTag()
    {
        var fixture = ForumFixture.CreateStudent();
        fixture.UnitOfWork.Set<ForumTag>().Items.Single(tag => tag.Id == fixture.TagId).IsActive = false;
        var service = fixture.CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreatePostAsync(new CreateForumPostRequest
        {
            Title = "Invalid tag",
            Content = "This should fail.",
            CategoryId = fixture.CategoryId,
            TagIds = new[] { fixture.TagId }
        }));
    }

    [Fact]
    public async Task PermissionService_GrantsForumCreateToStudentAndModerateToStaff()
    {
        var studentPermissions = new PermissionService(new TestCurrentUser(RoleNames.Student));
        var staffPermissions = new PermissionService(new TestCurrentUser(RoleNames.Staff));

        Assert.True(await studentPermissions.HasPermissionAsync(PermissionNames.ForumPostsCreate));
        Assert.False(await studentPermissions.HasPermissionAsync(PermissionNames.ForumPostsModerate));
        Assert.True(await staffPermissions.HasPermissionAsync(PermissionNames.ForumPostsModerate));
    }

    private sealed class ForumFixture
    {
        private ForumFixture(TestCurrentUser currentUser)
        {
            CurrentUser = currentUser;
            CurrentUserId = currentUser.UserId!.Value;
            UnitOfWork = new InMemoryUnitOfWork();
            CategoryId = Guid.NewGuid();
            TagId = Guid.NewGuid();
            AddRoles();
            UnitOfWork.Set<ForumCategory>().Items.Add(new ForumCategory
            {
                Id = CategoryId,
                Code = "housing",
                Name = "Housing",
                IsActive = true
            });
            UnitOfWork.Set<ForumTag>().Items.Add(new ForumTag
            {
                Id = TagId,
                Name = "Move-in",
                Slug = "move-in",
                IsActive = true
            });
            AddCurrentUser();
        }

        public InMemoryUnitOfWork UnitOfWork { get; }
        public TestCurrentUser CurrentUser { get; }
        public Guid CurrentUserId { get; }
        public Guid CategoryId { get; }
        public Guid TagId { get; }

        public static ForumFixture CreateStudent()
        {
            var studentId = Guid.NewGuid();
            return new ForumFixture(new TestCurrentUser(RoleNames.Student, studentId: studentId));
        }

        public static ForumFixture CreateManager() => new(new TestCurrentUser(RoleNames.Manager));

        public static ForumFixture CreateBuildingManager(Guid buildingId) =>
            new(new TestCurrentUser(RoleNames.BuildingManager, managerId: Guid.NewGuid(), buildingId: buildingId));

        public ForumService CreateService() => new(UnitOfWork, new AllowAllPermissionService(), CurrentUser, new RecordingAuditLogService());

        public void AddUser(Guid userId, string roleName, string fullName)
        {
            var role = UnitOfWork.Set<Role>().Items.Single(candidate => candidate.Name == roleName);
            UnitOfWork.Set<User>().Items.Add(new User
            {
                Id = userId,
                Username = fullName.Replace(" ", ".").ToLowerInvariant(),
                Email = userId.ToString("N")[..8] + "@ktx.local",
                FullName = fullName,
                RoleId = role.Id,
                Role = role,
                Status = UserStatus.Active
            });
        }

        public ForumPost AddPost(Guid authorUserId, ForumVisibilityScope scope, Guid? buildingId = null)
        {
            if (!UnitOfWork.Set<User>().Items.Any(user => user.Id == authorUserId))
            {
                AddUser(authorUserId, RoleNames.Student, "Student Author");
            }

            var post = new ForumPost
            {
                Id = Guid.NewGuid(),
                Title = "Seed post",
                Content = "Seed content",
                CategoryId = CategoryId,
                CreatedByUserId = authorUserId,
                VisibilityScope = scope,
                BuildingId = buildingId,
                TargetRoleName = scope == ForumVisibilityScope.Role ? RoleNames.Student : null,
                Status = ForumPostStatus.Published,
                CreatedAt = DateTime.UtcNow
            };
            UnitOfWork.Set<ForumPost>().Items.Add(post);
            return post;
        }

        private void AddCurrentUser()
        {
            AddUser(CurrentUserId, CurrentUser.Roles.Single(), CurrentUser.FullName ?? "Current User");
            if (CurrentUser.CurrentUser?.StudentId is { } studentId)
            {
                UnitOfWork.Set<Student>().Items.Add(new Student
                {
                    Id = studentId,
                    UserId = CurrentUserId,
                    StudentCode = "SV001",
                    FullName = "Nguyen Van An"
                });
            }

            if (CurrentUser.CurrentUser?.ManagerId is { } managerId)
            {
                UnitOfWork.Set<Manager>().Items.Add(new Manager
                {
                    Id = managerId,
                    UserId = CurrentUserId,
                    StaffCode = "BM001",
                    FullName = CurrentUser.FullName ?? "Building Manager",
                    IsBuildingManager = CurrentUser.IsInRole(RoleNames.BuildingManager),
                    BuildingId = CurrentUser.CurrentUser.BuildingId
                });
            }
        }

        private void AddRoles()
        {
            foreach (var roleName in new[] { RoleNames.Admin, RoleNames.Manager, RoleNames.BuildingManager, RoleNames.Staff, RoleNames.Student })
            {
                UnitOfWork.Set<Role>().Items.Add(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    IsSystemRole = true
                });
            }
        }
    }
}
