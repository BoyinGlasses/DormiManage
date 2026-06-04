using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Forum;
using DormitoryManagement.Application.Security;
using DormitoryManagement.Application.Services.Forum;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Tests;

public sealed class ForumPostServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenStudent_NormalizesTagsAndCreatesPendingPost()
    {
        var unitOfWork = new InMemoryUnitOfWork();
        var repository = new InMemoryForumPostRepository(unitOfWork);
        var currentUser = new TestCurrentUser(RoleNames.Student);
        var service = CreateService(repository, unitOfWork, currentUser);

        var result = await service.CreateAsync(new CreateForumPostRequest
        {
            Title = "  Wi-Fi Guide  ",
            Content = "Connect with your student code.",
            Category = " Guides ",
            Tags = ["#WiFi", "wifi", "  guide  ", ""]
        });

        Assert.True(result.Succeeded);
        var post = Assert.Single(unitOfWork.Set<ForumPost>().Items);
        Assert.Equal(ForumPostStatus.Pending, post.Status);
        Assert.Equal("Wi-Fi Guide", post.Title);
        Assert.Equal(new[] { "wifi", "guide" }, post.Tags.Select(tag => tag.Name).ToArray());
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CreateAsync_WhenManager_CreatesPublishedPost()
    {
        var unitOfWork = new InMemoryUnitOfWork();
        var service = CreateService(
            new InMemoryForumPostRepository(unitOfWork),
            unitOfWork,
            new TestCurrentUser(RoleNames.Manager));

        var result = await service.CreateAsync(new CreateForumPostRequest
        {
            Title = "Maintenance notice",
            Content = "Maintenance starts at 8 AM.",
            Category = "Announcements",
            Tags = ["maintenance"]
        });

        Assert.True(result.Succeeded, result.Error);
        Assert.Equal(ForumPostStatus.Published, Assert.Single(unitOfWork.Set<ForumPost>().Items).Status);
    }

    [Fact]
    public async Task CreateAsync_RejectsUnsupportedCategoryAndTag()
    {
        var unitOfWork = new InMemoryUnitOfWork();
        var service = CreateService(
            new InMemoryForumPostRepository(unitOfWork),
            unitOfWork,
            new TestCurrentUser(RoleNames.Student));

        var categoryResult = await service.CreateAsync(new CreateForumPostRequest
        {
            Title = "Question",
            Content = "Can I post this?",
            Category = "Random",
            Tags = ["wifi"]
        });
        var tagResult = await service.CreateAsync(new CreateForumPostRequest
        {
            Title = "Question",
            Content = "Can I post this?",
            Category = "Q&A",
            Tags = ["random"]
        });

        Assert.False(categoryResult.Succeeded);
        Assert.False(tagResult.Succeeded);
        Assert.Empty(unitOfWork.Set<ForumPost>().Items);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotAuthorOrManager_ReturnsFailure()
    {
        var unitOfWork = new InMemoryUnitOfWork();
        var authorId = Guid.NewGuid();
        var post = CreatePost(authorId);
        unitOfWork.Set<ForumPost>().Items.Add(post);
        var service = CreateService(new InMemoryForumPostRepository(unitOfWork), unitOfWork, new TestCurrentUser(RoleNames.Student));

        var result = await service.UpdateAsync(new UpdateForumPostRequest
        {
            Id = post.Id,
            Title = "Updated",
            Content = "Updated content",
            Category = "General"
        });

        Assert.False(result.Succeeded);
        Assert.Equal("Original", post.Title);
    }

    [Fact]
    public async Task DeleteAsync_WhenAuthor_SoftDeletesPost()
    {
        var unitOfWork = new InMemoryUnitOfWork();
        var userId = Guid.NewGuid();
        var post = CreatePost(userId);
        unitOfWork.Set<ForumPost>().Items.Add(post);
        var service = CreateService(new InMemoryForumPostRepository(unitOfWork), unitOfWork, new TestCurrentUser(RoleNames.Student, userId));

        var result = await service.DeleteAsync(post.Id);

        Assert.True(result.Succeeded);
        Assert.Equal(ForumPostStatus.Deleted, post.Status);
    }

    [Fact]
    public async Task GetFeedAsync_AppliesSearchFilterSortAndPagination()
    {
        var unitOfWork = new InMemoryUnitOfWork();
        var author = new User { Id = Guid.NewGuid(), FullName = "Author" };
        unitOfWork.Set<ForumPost>().Items.AddRange(
        [
            CreatePost(author.Id, "Wi-Fi setup", "Guides", ["wifi"], 10, DateTime.UtcNow.AddHours(-3), author),
            CreatePost(author.Id, "Cafeteria review", "General", ["food"], 30, DateTime.UtcNow.AddHours(-2), author),
            CreatePost(author.Id, "Wi-Fi outage", "Support", ["wifi"], 50, DateTime.UtcNow.AddHours(-1), author)
        ]);
        var service = CreateService(new InMemoryForumPostRepository(unitOfWork), unitOfWork, new TestCurrentUser(RoleNames.Student));

        var result = await service.GetFeedAsync(new ForumPostFilterRequest
        {
            SearchText = "wifi",
            Tag = "wifi",
            SortBy = ForumPostSortOption.MostViewed,
            PageNumber = 1,
            PageSize = 1
        });

        Assert.Equal(2, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Wi-Fi outage", result.Items[0].Title);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(1, result.PageSize);
    }

    [Fact]
    public async Task GetFeedAsync_AppliesVisibilityScope()
    {
        var buildingA = Guid.NewGuid();
        var buildingB = Guid.NewGuid();
        var roomA = Guid.NewGuid();
        var unitOfWork = new InMemoryUnitOfWork();
        var author = new User { Id = Guid.NewGuid(), FullName = "Author" };
        unitOfWork.Set<ForumPost>().Items.AddRange(
        [
            CreatePost(author.Id, "Dormitory", author: author),
            CreatePost(author.Id, "Building A", author: author, visibilityScope: ForumVisibilityScope.Building, buildingId: buildingA),
            CreatePost(author.Id, "Building B", author: author, visibilityScope: ForumVisibilityScope.Building, buildingId: buildingB),
            CreatePost(author.Id, "Room A", author: author, visibilityScope: ForumVisibilityScope.Room, roomId: roomA),
            CreatePost(author.Id, "Managers only", author: author, visibilityScope: ForumVisibilityScope.Role, roleName: RoleNames.Manager)
        ]);
        var service = CreateService(
            new InMemoryForumPostRepository(unitOfWork),
            unitOfWork,
            new TestCurrentUser(RoleNames.Student, buildingId: buildingA, currentRoomId: roomA));

        var result = await service.GetFeedAsync(new ForumPostFilterRequest());

        Assert.Equal(["Room A", "Building A", "Dormitory"], result.Items.Select(post => post.Title).ToArray());
    }

    [Fact]
    public async Task ManagerCanUpdateAnotherAuthorPost()
    {
        var unitOfWork = new InMemoryUnitOfWork();
        var post = CreatePost(Guid.NewGuid());
        unitOfWork.Set<ForumPost>().Items.Add(post);
        var service = CreateService(
            new InMemoryForumPostRepository(unitOfWork),
            unitOfWork,
            new TestCurrentUser(RoleNames.Manager));

        var result = await service.UpdateAsync(new UpdateForumPostRequest
        {
            Id = post.Id,
            Title = "Updated",
            Content = "Updated content",
            Category = "General",
            Tags = ["community"]
        });

        Assert.True(result.Succeeded, result.Error);
        Assert.Equal("Updated", post.Title);
    }

    [Fact]
    public async Task ApproveAsync_WhenManager_ApprovesPendingPost()
    {
        var unitOfWork = new InMemoryUnitOfWork();
        var post = CreatePost(Guid.NewGuid(), status: ForumPostStatus.Pending);
        unitOfWork.Set<ForumPost>().Items.Add(post);
        var service = CreateService(
            new InMemoryForumPostRepository(unitOfWork),
            unitOfWork,
            new TestCurrentUser(RoleNames.Manager));

        var result = await service.ApproveAsync(post.Id);

        Assert.True(result.Succeeded, result.Error);
        Assert.Equal(ForumPostStatus.Published, post.Status);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task ApproveAsync_WhenStudent_ReturnsFailure()
    {
        var unitOfWork = new InMemoryUnitOfWork();
        var post = CreatePost(Guid.NewGuid(), status: ForumPostStatus.Pending);
        unitOfWork.Set<ForumPost>().Items.Add(post);
        var service = CreateService(
            new InMemoryForumPostRepository(unitOfWork),
            unitOfWork,
            new TestCurrentUser(RoleNames.Student));

        var result = await service.ApproveAsync(post.Id);

        Assert.False(result.Succeeded);
        Assert.Equal(ForumPostStatus.Pending, post.Status);
    }

    private static ForumPostService CreateService(
        IForumPostRepository repository,
        InMemoryUnitOfWork unitOfWork,
        TestCurrentUser currentUser) =>
        new(repository, unitOfWork, currentUser, new ForumPermissionService(currentUser));

    private static ForumPost CreatePost(
        Guid authorId,
        string title = "Original",
        string category = "General",
        IReadOnlyCollection<string>? tags = null,
        int views = 0,
        DateTime? createdAt = null,
        User? author = null,
        ForumVisibilityScope visibilityScope = ForumVisibilityScope.Dormitory,
        Guid? buildingId = null,
        Guid? roomId = null,
        string? roleName = null,
        ForumPostStatus status = ForumPostStatus.Published) =>
        new()
        {
            Id = Guid.NewGuid(),
            AuthorUserId = authorId,
            AuthorUser = author,
            Title = title,
            Content = title + " content",
            Excerpt = title,
            Category = category,
            Status = status,
            VisibilityScope = visibilityScope,
            VisibilityBuildingId = buildingId,
            VisibilityRoomId = roomId,
            VisibilityRoleName = roleName,
            ViewCount = views,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            Tags = (tags ?? Array.Empty<string>()).Select(tag => new ForumPostTag { Id = Guid.NewGuid(), Name = tag }).ToList()
        };

    private sealed class InMemoryForumPostRepository : IForumPostRepository
    {
        private readonly InMemoryUnitOfWork _unitOfWork;

        public InMemoryForumPostRepository(InMemoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IQueryable<ForumPost> Query() => _unitOfWork.Set<ForumPost>().Items.AsQueryable();

        public Task<ForumPost?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(Query().FirstOrDefault(post => post.Id == id));

        public Task<PagedResult<ForumPost>> GetFeedAsync(ForumPostFilterRequest filter, CancellationToken ct = default)
        {
            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Clamp(filter.PageSize, 1, 50);
            var query = Query().Where(post => post.Status == ForumPostStatus.Published);
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                query = query.Where(post => post.Title.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase)
                    || post.Content.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase)
                    || post.Excerpt.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase)
                    || post.Category.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase)
                    || post.Tags.Any(tag => tag.Name.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Tag))
            {
                query = query.Where(post => post.Tags.Any(tag => tag.Name == filter.Tag));
            }

            if (!filter.CanViewAllScopes)
            {
                query = query.Where(post =>
                    post.VisibilityScope == ForumVisibilityScope.Dormitory ||
                    (post.VisibilityScope == ForumVisibilityScope.Building &&
                     filter.CurrentUserBuildingId.HasValue &&
                     post.VisibilityBuildingId == filter.CurrentUserBuildingId.Value) ||
                    (post.VisibilityScope == ForumVisibilityScope.Room &&
                     filter.CurrentUserRoomId.HasValue &&
                     post.VisibilityRoomId == filter.CurrentUserRoomId.Value) ||
                    (post.VisibilityScope == ForumVisibilityScope.Role &&
                     !string.IsNullOrWhiteSpace(filter.CurrentUserRoleName) &&
                     post.VisibilityRoleName == filter.CurrentUserRoleName));
            }

            query = filter.SortBy is ForumPostSortOption.MostViewed or ForumPostSortOption.Popular
                ? query.OrderByDescending(post => post.ViewCount)
                : query.OrderByDescending(post => post.CreatedAt);
            var items = query.ToList();
            return Task.FromResult(new PagedResult<ForumPost>(
                items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArray(),
                items.Count,
                pageNumber,
                pageSize));
        }

        public Task AddAsync(ForumPost post, CancellationToken ct = default)
        {
            _unitOfWork.Set<ForumPost>().Items.Add(post);
            return Task.CompletedTask;
        }

        public void Update(ForumPost post)
        {
        }
    }

    private sealed class ForumPermissionService : IPermissionService
    {
        private readonly TestCurrentUser _currentUser;

        public ForumPermissionService(TestCurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public Task<bool> HasPermissionAsync(string permissionName, CancellationToken ct = default)
        {
            if (_currentUser.IsInRole(RoleNames.Admin))
            {
                return Task.FromResult(true);
            }

            var allowed = permissionName switch
            {
                PermissionNames.ForumRead or PermissionNames.ForumCreate or PermissionNames.ForumManageOwn => true,
                PermissionNames.ForumModerate when _currentUser.IsInRole(RoleNames.Manager) => true,
                _ => false
            };
            return Task.FromResult(allowed);
        }

        public async Task<AuthorizationResult> AuthorizeAsync(string permissionName, CancellationToken ct = default) =>
            await HasPermissionAsync(permissionName, ct)
                ? AuthorizationResult.Success()
                : AuthorizationResult.Denied("Denied");

        public async Task EnsurePermissionAsync(string permissionName, CancellationToken ct = default)
        {
            if (!await HasPermissionAsync(permissionName, ct))
            {
                throw new AccessDeniedException("Denied");
            }
        }
    }
}
