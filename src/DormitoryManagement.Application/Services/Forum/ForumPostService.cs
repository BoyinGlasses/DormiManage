using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Forum;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Forum;

public sealed class ForumPostService : IForumPostService
{
    private readonly IForumPostRepository _posts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ForumPostService(IForumPostRepository posts, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _posts = posts;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ForumPostListItemDto>> GetFeedAsync(ForumPostFilterRequest request, CancellationToken ct = default)
    {
        var feed = await _posts.GetFeedAsync(NormalizeFilter(request), ct);
        return new PagedResult<ForumPostListItemDto>(
            feed.Items.Select(MapListItem).ToArray(),
            feed.TotalCount,
            feed.PageNumber,
            feed.PageSize);
    }

    public async Task<Result<ForumPostDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _posts.GetByIdWithDetailsAsync(id, ct);
        if (post is null || post.Status is ForumPostStatus.Deleted)
        {
            return Result<ForumPostDto>.Failure("Forum post was not found.");
        }

        if (post.Status is ForumPostStatus.Published)
        {
            post.ViewCount++;
            _posts.Update(post);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return Result<ForumPostDto>.Success(MapDetail(post));
    }

    public async Task<Result<ForumPostDto>> CreateAsync(CreateForumPostRequest request, CancellationToken ct = default)
    {
        if (!EnsureAuthenticated(out var userId, out var error))
        {
            return Result<ForumPostDto>.Failure(error);
        }

        var validation = Validate(request.Title, request.Content, request.Category, request.Area);
        if (validation is not null)
        {
            return Result<ForumPostDto>.Failure(validation);
        }

        var post = new ForumPost
        {
            Id = Guid.NewGuid(),
            AuthorUserId = userId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            Excerpt = CreateExcerpt(request.Content, request.Excerpt),
            Category = request.Category.Trim(),
            Area = NormalizeOptional(request.Area),
            Status = ForumPostStatus.Published,
            IsPinned = request.IsPinned,
            IsImportant = request.IsImportant,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserName
        };
        ReplaceTags(post, request.Tags);

        await _posts.AddAsync(post, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ForumPostDto>.Success(MapDetail(post));
    }

    public async Task<Result<ForumPostDto>> UpdateAsync(UpdateForumPostRequest request, CancellationToken ct = default)
    {
        var post = await _posts.GetByIdWithDetailsAsync(request.Id, ct);
        if (post is null)
        {
            return Result<ForumPostDto>.Failure("Forum post was not found.");
        }

        if (post.Status is ForumPostStatus.Deleted)
        {
            return Result<ForumPostDto>.Failure("Deleted posts cannot be updated.");
        }

        if (!CanManagePost(post))
        {
            return Result<ForumPostDto>.Failure("You do not have permission to update this post.");
        }

        var validation = Validate(request.Title, request.Content, request.Category, request.Area);
        if (validation is not null)
        {
            return Result<ForumPostDto>.Failure(validation);
        }

        post.Title = request.Title.Trim();
        post.Content = request.Content.Trim();
        post.Excerpt = CreateExcerpt(request.Content, request.Excerpt);
        post.Category = request.Category.Trim();
        post.Area = NormalizeOptional(request.Area);
        post.IsPinned = request.IsPinned;
        post.IsImportant = request.IsImportant;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = _currentUser.UserName;
        ReplaceTags(post, request.Tags);

        _posts.Update(post);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ForumPostDto>.Success(MapDetail(post));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _posts.GetByIdWithDetailsAsync(id, ct);
        if (post is null)
        {
            return Result.Failure("Forum post was not found.");
        }

        if (!CanManagePost(post))
        {
            return Result.Failure("You do not have permission to delete this post.");
        }

        post.Status = ForumPostStatus.Deleted;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = _currentUser.UserName;
        _posts.Update(post);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> HideAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _posts.GetByIdWithDetailsAsync(id, ct);
        if (post is null)
        {
            return Result.Failure("Forum post was not found.");
        }

        if (!IsAdminOrManager())
        {
            return Result.Failure("Only Admin or Manager can hide posts.");
        }

        post.Status = ForumPostStatus.Hidden;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = _currentUser.UserName;
        _posts.Update(post);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static ForumPostFilterRequest NormalizeFilter(ForumPostFilterRequest request) => new()
    {
        SearchText = NormalizeOptional(request.SearchText),
        Category = NormalizeOptional(request.Category),
        Tag = NormalizeTag(request.Tag),
        Area = NormalizeOptional(request.Area),
        SortBy = request.SortBy,
        PageNumber = Math.Max(1, request.PageNumber),
        PageSize = Math.Clamp(request.PageSize, 1, 50)
    };

    private bool EnsureAuthenticated(out Guid userId, out string error)
    {
        if (_currentUser.IsAuthenticated && _currentUser.UserId is { } currentUserId)
        {
            userId = currentUserId;
            error = string.Empty;
            return true;
        }

        userId = Guid.Empty;
        error = "Current user must be authenticated.";
        return false;
    }

    private bool CanManagePost(ForumPost post) =>
        _currentUser.UserId == post.AuthorUserId || IsAdminOrManager();

    private bool IsAdminOrManager() =>
        _currentUser.IsInRole(RoleNames.Admin) || _currentUser.IsInRole(RoleNames.Manager);

    private static string? Validate(string title, string content, string category, string? area)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Title is required.";
        }

        if (title.Trim().Length > 200)
        {
            return "Title must be 200 characters or fewer.";
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return "Content is required.";
        }

        if (content.Trim().Length > 8000)
        {
            return "Content must be 8000 characters or fewer.";
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            return "Category is required.";
        }

        if (category.Trim().Length > 100)
        {
            return "Category must be 100 characters or fewer.";
        }

        if (!string.IsNullOrWhiteSpace(area) && area.Trim().Length > 100)
        {
            return "Area must be 100 characters or fewer.";
        }

        return null;
    }

    private static void ReplaceTags(ForumPost post, IEnumerable<string> tags)
    {
        post.Tags.Clear();
        foreach (var tag in NormalizeTags(tags))
        {
            post.Tags.Add(new ForumPostTag
            {
                Id = Guid.NewGuid(),
                ForumPostId = post.Id,
                Name = tag
            });
        }
    }

    private static IReadOnlyList<string> NormalizeTags(IEnumerable<string> tags) =>
        tags
            .Select(NormalizeTag)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()!;

    private static string? NormalizeTag(string? tag)
    {
        var normalized = NormalizeOptional(tag)?.TrimStart('#').Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static string CreateExcerpt(string content, string? excerpt)
    {
        var source = string.IsNullOrWhiteSpace(excerpt) ? content : excerpt;
        var normalized = string.Join(" ", source.Trim().Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length <= 500 ? normalized : normalized[..497] + "...";
    }

    private ForumPostDto MapDetail(ForumPost post)
    {
        var dto = new ForumPostDto
        {
            Content = post.Content
        };
        ApplyListFields(dto, post);
        return dto;
    }

    private ForumPostListItemDto MapListItem(ForumPost post)
    {
        var dto = new ForumPostListItemDto();
        ApplyListFields(dto, post);
        return dto;
    }

    private static void ApplyListFields(ForumPostListItemDto dto, ForumPost post)
    {
        dto.Id = post.Id;
        dto.Title = post.Title;
        dto.Excerpt = post.Excerpt;
        dto.Category = post.Category;
        dto.Area = post.Area;
        dto.IsPinned = post.IsPinned;
        dto.IsImportant = post.IsImportant;
        dto.ViewCount = post.ViewCount;
        dto.LikeCount = 0;
        dto.CommentCount = 0;
        dto.IsLikedByCurrentUser = false;
        dto.AuthorUserId = post.AuthorUserId;
        dto.AuthorName = post.AuthorUser?.FullName ?? post.AuthorUser?.Username ?? "User";
        dto.AuthorRole = post.AuthorUser?.Role?.Name ?? string.Empty;
        dto.CreatedAt = post.CreatedAt;
        dto.Tags = post.Tags.Select(tag => tag.Name).OrderBy(tag => tag).ToArray();
    }
}
