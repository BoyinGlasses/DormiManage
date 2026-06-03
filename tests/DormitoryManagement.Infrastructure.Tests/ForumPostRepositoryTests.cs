using DormitoryManagement.Application.DTOs.Forum;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.Infrastructure.Data;
using DormitoryManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Tests;

public sealed class ForumPostRepositoryTests
{
    [Fact]
    public async Task GetFeedAsync_ReturnsPublishedPostsOnlyAndFiltersByTag()
    {
        await using var dbContext = CreateDbContext();
        var author = await SeedAuthorAsync(dbContext);
        dbContext.ForumPosts.AddRange(
            CreatePost(author.Id, "Wi-Fi setup", ForumPostStatus.Published, ["wifi"], 10),
            CreatePost(author.Id, "Hidden Wi-Fi note", ForumPostStatus.Hidden, ["wifi"], 50),
            CreatePost(author.Id, "Cafeteria review", ForumPostStatus.Published, ["food"], 20));
        await dbContext.SaveChangesAsync();
        var repository = new ForumPostRepository(dbContext);

        var result = await repository.GetFeedAsync(new ForumPostFilterRequest { Tag = "wifi" });

        var post = Assert.Single(result.Items);
        Assert.Equal("Wi-Fi setup", post.Title);
        Assert.Equal(1, result.TotalCount);
        Assert.NotNull(post.AuthorUser);
        Assert.Contains(post.Tags, tag => tag.Name == "wifi");
    }

    [Fact]
    public async Task GetFeedAsync_SortsMostViewedAndClampsPaging()
    {
        await using var dbContext = CreateDbContext();
        var author = await SeedAuthorAsync(dbContext);
        dbContext.ForumPosts.AddRange(
            CreatePost(author.Id, "Low views", ForumPostStatus.Published, ["general"], 5),
            CreatePost(author.Id, "High views", ForumPostStatus.Published, ["general"], 80));
        await dbContext.SaveChangesAsync();
        var repository = new ForumPostRepository(dbContext);

        var result = await repository.GetFeedAsync(new ForumPostFilterRequest
        {
            SortBy = ForumPostSortOption.MostViewed,
            PageNumber = 0,
            PageSize = 99
        });

        Assert.Equal(1, result.PageNumber);
        Assert.Equal(50, result.PageSize);
        Assert.Equal("High views", result.Items[0].Title);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_LoadsAuthorAndTags()
    {
        await using var dbContext = CreateDbContext();
        var author = await SeedAuthorAsync(dbContext);
        var post = CreatePost(author.Id, "Detail", ForumPostStatus.Published, ["detail"], 1);
        dbContext.ForumPosts.Add(post);
        await dbContext.SaveChangesAsync();
        var repository = new ForumPostRepository(dbContext);

        var result = await repository.GetByIdWithDetailsAsync(post.Id);

        Assert.NotNull(result);
        Assert.NotNull(result!.AuthorUser);
        Assert.Single(result.Tags);
    }

    private static DormitoryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DormitoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DormitoryDbContext(options);
    }

    private static async Task<User> SeedAuthorAsync(DormitoryDbContext dbContext)
    {
        var role = new Role { Id = Guid.NewGuid(), Name = "Student" };
        var author = new User
        {
            Id = Guid.NewGuid(),
            Username = "student",
            Email = "student@ktx.local",
            FullName = "Student User",
            RoleId = role.Id,
            Role = role
        };
        dbContext.Roles.Add(role);
        dbContext.Users.Add(author);
        await dbContext.SaveChangesAsync();
        return author;
    }

    private static ForumPost CreatePost(Guid authorId, string title, ForumPostStatus status, IReadOnlyCollection<string> tags, int viewCount) =>
        new()
        {
            Id = Guid.NewGuid(),
            AuthorUserId = authorId,
            Title = title,
            Content = title + " content",
            Excerpt = title,
            Category = "General",
            Status = status,
            ViewCount = viewCount,
            CreatedAt = DateTime.UtcNow.AddMinutes(-viewCount),
            Tags = tags.Select(tag => new ForumPostTag { Id = Guid.NewGuid(), Name = tag }).ToList()
        };
}
