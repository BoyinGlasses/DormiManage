using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Forum;
using DormitoryManagement.Application.Services.Forum;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Tests;

public sealed class ForumPostServiceTests
{
    [Fact]
    public async Task CreateAsync_NormalizesTagsAndCreatesPublishedPost()
    {
        var unitOfWork = new InMemoryUnitOfWork();
        var repository = new InMemoryForumPostRepository(unitOfWork);
        var currentUser = new TestCurrentUser(RoleNames.Student);
        var service = new ForumPostService(repository, unitOfWork, currentUser);

        var result = await service.CreateAsync(new CreateForumPostRequest
        {
            Title = "  Wi-Fi Guide  ",
            Content = "Connect with your student code.",
            Category = " Guides ",
            Tags = ["#WiFi", "wifi", "  Help  ", ""]
        });

        Assert.True(result.Succeeded);
        var post = Assert.Single(unitOfWork.Set<ForumPost>().Items);
        Assert.Equal(ForumPostStatus.Published, post.Status);
        Assert.Equal("Wi-Fi Guide", post.Title);
        Assert.Equal(new[] { "wifi", "help" }, post.Tags.Select(tag => tag.Name).ToArray());
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotAuthorOrManager_ReturnsFailure()
    {
        var unitOfWork = new InMemoryUnitOfWork();
        var authorId = Guid.NewGuid();
        var post = CreatePost(authorId);
        unitOfWork.Set<ForumPost>().Items.Add(post);
        var service = new ForumPostService(new InMemoryForumPostRepository(unitOfWork), unitOfWork, new TestCurrentUser(RoleNames.Student));

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
        var service = new ForumPostService(new InMemoryForumPostRepository(unitOfWork), unitOfWork, new TestCurrentUser(RoleNames.Student, userId));

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
        var service = new ForumPostService(new InMemoryForumPostRepository(unitOfWork), unitOfWork, new TestCurrentUser(RoleNames.Student));

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

    private static ForumPost CreatePost(Guid authorId, string title = "Original", string category = "General", IReadOnlyCollection<string>? tags = null, int views = 0, DateTime? createdAt = null, User? author = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            AuthorUserId = authorId,
            AuthorUser = author,
            Title = title,
            Content = title + " content",
            Excerpt = title,
            Category = category,
            Status = ForumPostStatus.Published,
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
}
