using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Forum;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Repositories;

public sealed class ForumPostRepository : IForumPostRepository
{
    private readonly DormitoryDbContext _dbContext;

    public ForumPostRepository(DormitoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<ForumPost> Query() => _dbContext.ForumPosts.AsQueryable();

    public Task<ForumPost?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.ForumPosts
            .Include(post => post.Tags)
            .Include(post => post.Comments)
            .Include(post => post.Reactions)
            .Include(post => post.AuthorUser)
            .ThenInclude(user => user!.Role)
            .FirstOrDefaultAsync(post => post.Id == id, ct);

    public async Task<PagedResult<ForumPost>> GetFeedAsync(ForumPostFilterRequest filter, CancellationToken ct = default)
    {
        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 50);
        var query = _dbContext.ForumPosts
            .AsNoTracking()
            .Include(post => post.Tags)
            .Include(post => post.Comments)
            .Include(post => post.Reactions)
            .Include(post => post.AuthorUser)
            .ThenInclude(user => user!.Role)
            .Where(post => post.Status == ForumPostStatus.Published);

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var searchText = filter.SearchText.Trim();
            query = query.Where(post =>
                post.Title.Contains(searchText) ||
                post.Content.Contains(searchText) ||
                post.Excerpt.Contains(searchText) ||
                post.Category.Contains(searchText) ||
                post.Tags.Any(tag => tag.Name.Contains(searchText)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            var category = filter.Category.Trim();
            query = query.Where(post => post.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(filter.Tag))
        {
            var tag = filter.Tag.Trim().TrimStart('#').ToLowerInvariant();
            query = query.Where(post => post.Tags.Any(candidate => candidate.Name == tag));
        }

        if (!string.IsNullOrWhiteSpace(filter.Area))
        {
            var area = filter.Area.Trim();
            query = query.Where(post => post.Area == area);
        }

        if (!filter.CanViewAllScopes)
        {
            var roleName = filter.CurrentUserRoleName?.Trim();
            query = query.Where(post =>
                post.VisibilityScope == ForumVisibilityScope.Dormitory ||
                (post.VisibilityScope == ForumVisibilityScope.Building &&
                 filter.CurrentUserBuildingId.HasValue &&
                 post.VisibilityBuildingId == filter.CurrentUserBuildingId.Value) ||
                (post.VisibilityScope == ForumVisibilityScope.Room &&
                 filter.CurrentUserRoomId.HasValue &&
                 post.VisibilityRoomId == filter.CurrentUserRoomId.Value) ||
                (post.VisibilityScope == ForumVisibilityScope.Role &&
                 roleName != null &&
                 post.VisibilityRoleName == roleName));
        }

        query = filter.SortBy switch
        {
            ForumPostSortOption.MostViewed or ForumPostSortOption.Popular => query
                .OrderByDescending(post => post.IsPinned)
                .ThenByDescending(post => post.ViewCount)
                .ThenByDescending(post => post.CreatedAt),
            _ => query
                .OrderByDescending(post => post.IsPinned)
                .ThenByDescending(post => post.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<ForumPost>(items, totalCount, pageNumber, pageSize);
    }

    public Task AddAsync(ForumPost post, CancellationToken ct = default) =>
        _dbContext.ForumPosts.AddAsync(post, ct).AsTask();

    public void Update(ForumPost post) => _dbContext.ForumPosts.Update(post);
}
