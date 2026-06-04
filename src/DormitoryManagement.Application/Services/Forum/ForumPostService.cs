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
    private readonly IPermissionService _permissions;

    public ForumPostService(
        IForumPostRepository posts,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IPermissionService permissions)
    {
        _posts = posts;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _permissions = permissions;
    }

    public async Task<PagedResult<ForumPostListItemDto>> GetFeedAsync(ForumPostFilterRequest request, CancellationToken ct = default)
    {
        if (!await _permissions.HasPermissionAsync(PermissionNames.ForumRead, ct) &&
            !await _permissions.HasPermissionAsync(PermissionNames.ForumModerate, ct))
        {
            return new PagedResult<ForumPostListItemDto>(Array.Empty<ForumPostListItemDto>(), 0, Math.Max(1, request.PageNumber), Math.Clamp(request.PageSize, 1, 50));
        }

        var feed = await _posts.GetFeedAsync(await NormalizeFilterAsync(request, ct), ct);
        return new PagedResult<ForumPostListItemDto>(
            feed.Items.Select(MapListItem).ToArray(),
            feed.TotalCount,
            feed.PageNumber,
            feed.PageSize);
    }

    public async Task<IReadOnlyList<ForumPostListItemDto>> GetPendingAsync(CancellationToken ct = default)
    {
        if (!await _permissions.HasPermissionAsync(PermissionNames.ForumModerate, ct))
        {
            return Array.Empty<ForumPostListItemDto>();
        }

        return _posts.Query()
            .Where(post => post.Status == ForumPostStatus.Pending)
            .OrderBy(post => post.CreatedAt)
            .ToArray()
            .Select(MapListItem)
            .ToArray();
    }

    public async Task<Result<ForumPostDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _posts.GetByIdWithDetailsAsync(id, ct);
        if (post is null || post.Status is ForumPostStatus.Deleted)
        {
            return Result<ForumPostDto>.Failure("Forum post was not found.");
        }

        if (!await CanViewPostAsync(post, ct))
        {
            return Result<ForumPostDto>.Failure("Forum post was not found or is not visible.");
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

        if (!await _permissions.HasPermissionAsync(PermissionNames.ForumCreate, ct))
        {
            return Result<ForumPostDto>.Failure("You do not have permission to create forum posts.");
        }

        var validation = Validate(request.Title, request.Content, request.Category, request.Area, request.Tags)
            ?? ValidateVisibility(request.VisibilityScope, request.VisibilityBuildingId, request.VisibilityRoomId, request.VisibilityRoleName);
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
            Category = NormalizeCategory(request.Category),
            Area = NormalizeOptional(request.Area),
            VisibilityScope = request.VisibilityScope,
            VisibilityBuildingId = request.VisibilityScope == ForumVisibilityScope.Building ? request.VisibilityBuildingId : null,
            VisibilityRoomId = request.VisibilityScope == ForumVisibilityScope.Room ? request.VisibilityRoomId : null,
            VisibilityRoleName = request.VisibilityScope == ForumVisibilityScope.Role ? NormalizeRoleName(request.VisibilityRoleName) : null,
            Status = await _permissions.HasPermissionAsync(PermissionNames.ForumModerate, ct)
                ? ForumPostStatus.Published
                : ForumPostStatus.Pending,
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

        if (!await CanManagePostAsync(post, ct))
        {
            return Result<ForumPostDto>.Failure("You do not have permission to update this post.");
        }

        var validation = Validate(request.Title, request.Content, request.Category, request.Area, request.Tags)
            ?? ValidateVisibility(request.VisibilityScope, request.VisibilityBuildingId, request.VisibilityRoomId, request.VisibilityRoleName);
        if (validation is not null)
        {
            return Result<ForumPostDto>.Failure(validation);
        }

        post.Title = request.Title.Trim();
        post.Content = request.Content.Trim();
        post.Excerpt = CreateExcerpt(request.Content, request.Excerpt);
        post.Category = NormalizeCategory(request.Category);
        post.Area = NormalizeOptional(request.Area);
        post.VisibilityScope = request.VisibilityScope;
        post.VisibilityBuildingId = request.VisibilityScope == ForumVisibilityScope.Building ? request.VisibilityBuildingId : null;
        post.VisibilityRoomId = request.VisibilityScope == ForumVisibilityScope.Room ? request.VisibilityRoomId : null;
        post.VisibilityRoleName = request.VisibilityScope == ForumVisibilityScope.Role ? NormalizeRoleName(request.VisibilityRoleName) : null;
        post.IsPinned = request.IsPinned;
        post.IsImportant = request.IsImportant;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = _currentUser.UserName;
        ReplaceTags(post, request.Tags);

        _posts.Update(post);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ForumPostDto>.Success(MapDetail(post));
    }

    public async Task<Result> ApproveAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _posts.GetByIdWithDetailsAsync(id, ct);
        if (post is null)
        {
            return Result.Failure("Forum post was not found.");
        }

        if (!await _permissions.HasPermissionAsync(PermissionNames.ForumModerate, ct))
        {
            return Result.Failure("Only Admin or Manager can approve posts.");
        }

        if (post.Status != ForumPostStatus.Pending)
        {
            return Result.Failure("Only pending posts can be approved.");
        }

        post.Status = ForumPostStatus.Published;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = _currentUser.UserName;
        _posts.Update(post);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _posts.GetByIdWithDetailsAsync(id, ct);
        if (post is null)
        {
            return Result.Failure("Forum post was not found.");
        }

        if (!await CanManagePostAsync(post, ct))
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

        if (!await _permissions.HasPermissionAsync(PermissionNames.ForumModerate, ct))
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

    private async Task<ForumPostFilterRequest> NormalizeFilterAsync(ForumPostFilterRequest request, CancellationToken ct) => new()
    {
        SearchText = NormalizeOptional(request.SearchText),
        Category = NormalizeCategoryFilter(request.Category),
        Tag = NormalizeTag(request.Tag),
        Area = NormalizeOptional(request.Area),
        SortBy = request.SortBy,
        PageNumber = Math.Max(1, request.PageNumber),
        PageSize = Math.Clamp(request.PageSize, 1, 50),
        CurrentUserRoleName = _currentUser.CurrentUser?.RoleName,
        CurrentUserBuildingId = _currentUser.CurrentUser?.BuildingId,
        CurrentUserRoomId = _currentUser.CurrentUser?.CurrentRoomId,
        CanViewAllScopes = await _permissions.HasPermissionAsync(PermissionNames.ForumModerate, ct)
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

    private async Task<bool> CanManagePostAsync(ForumPost post, CancellationToken ct) =>
        (_currentUser.UserId == post.AuthorUserId && await _permissions.HasPermissionAsync(PermissionNames.ForumManageOwn, ct)) ||
        await _permissions.HasPermissionAsync(PermissionNames.ForumModerate, ct);

    private async Task<bool> CanViewPostAsync(ForumPost post, CancellationToken ct)
    {
        if (await _permissions.HasPermissionAsync(PermissionNames.ForumModerate, ct))
        {
            return true;
        }

        if (_currentUser.UserId == post.AuthorUserId && post.Status == ForumPostStatus.Pending)
        {
            return await _permissions.HasPermissionAsync(PermissionNames.ForumManageOwn, ct);
        }

        return post.Status == ForumPostStatus.Published && post.VisibilityScope switch
        {
            ForumVisibilityScope.Dormitory => await _permissions.HasPermissionAsync(PermissionNames.ForumRead, ct),
            ForumVisibilityScope.Building => post.VisibilityBuildingId.HasValue &&
                                             post.VisibilityBuildingId == _currentUser.CurrentUser?.BuildingId,
            ForumVisibilityScope.Room => post.VisibilityRoomId.HasValue &&
                                         post.VisibilityRoomId == _currentUser.CurrentUser?.CurrentRoomId,
            ForumVisibilityScope.Role => !string.IsNullOrWhiteSpace(post.VisibilityRoleName) &&
                                         _currentUser.IsInRole(post.VisibilityRoleName),
            _ => false
        };
    }

    private static string? Validate(string title, string content, string category, string? area, IEnumerable<string> tags)
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

        if (!ForumCatalog.Categories.Contains(category.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return "Category is not supported.";
        }

        if (!string.IsNullOrWhiteSpace(area) && area.Trim().Length > 100)
        {
            return "Area must be 100 characters or fewer.";
        }

        var unsupportedTag = NormalizeTags(tags).FirstOrDefault(tag => !ForumCatalog.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
        return unsupportedTag is null ? null : $"Tag '{unsupportedTag}' is not supported.";
    }

    private static string? ValidateVisibility(
        ForumVisibilityScope scope,
        Guid? buildingId,
        Guid? roomId,
        string? roleName)
    {
        return scope switch
        {
            ForumVisibilityScope.Building when !buildingId.HasValue => "Building visibility requires a building.",
            ForumVisibilityScope.Room when !roomId.HasValue => "Room visibility requires a room.",
            ForumVisibilityScope.Role when string.IsNullOrWhiteSpace(roleName) => "Role visibility requires a role.",
            ForumVisibilityScope.Role when !KnownRoles.Contains(roleName.Trim(), StringComparer.OrdinalIgnoreCase) => "Role visibility is not supported.",
            _ => null
        };
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

    private static string NormalizeCategory(string category) =>
        ForumCatalog.Categories.First(candidate => string.Equals(candidate, category.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? NormalizeCategoryFilter(string? category) =>
        string.IsNullOrWhiteSpace(category) ? null : NormalizeCategory(category);

    private static string? NormalizeRoleName(string? roleName) =>
        KnownRoles.FirstOrDefault(candidate => string.Equals(candidate, roleName?.Trim(), StringComparison.OrdinalIgnoreCase));

    private static readonly string[] KnownRoles = [RoleNames.Admin, RoleNames.Manager, RoleNames.Student];

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
        ApplyListFields(dto, post, _currentUser.UserId);
        return dto;
    }

    private ForumPostListItemDto MapListItem(ForumPost post)
    {
        var dto = new ForumPostListItemDto();
        ApplyListFields(dto, post, _currentUser.UserId);
        return dto;
    }

    private static void ApplyListFields(ForumPostListItemDto dto, ForumPost post, Guid? currentUserId)
    {
        dto.Id = post.Id;
        dto.Title = post.Title;
        dto.Excerpt = post.Excerpt;
        dto.Category = post.Category;
        dto.Area = post.Area;
        dto.Status = post.Status;
        dto.VisibilityScope = post.VisibilityScope;
        dto.VisibilityBuildingId = post.VisibilityBuildingId;
        dto.VisibilityRoomId = post.VisibilityRoomId;
        dto.VisibilityRoleName = post.VisibilityRoleName;
        dto.IsPinned = post.IsPinned;
        dto.IsImportant = post.IsImportant;
        dto.ViewCount = post.ViewCount;
        dto.LikeCount = post.Reactions.Count;
        dto.CommentCount = post.Comments.Count(comment => comment.Status == ForumCommentStatus.Published);
        dto.IsLikedByCurrentUser = currentUserId.HasValue && post.Reactions.Any(reaction => reaction.UserId == currentUserId.Value);
        dto.AuthorUserId = post.AuthorUserId;
        dto.AuthorName = post.AuthorUser?.FullName ?? post.AuthorUser?.Username ?? "User";
        dto.AuthorRole = post.AuthorUser?.Role?.Name ?? string.Empty;
        dto.CreatedAt = post.CreatedAt;
        dto.Tags = post.Tags.Select(tag => tag.Name).OrderBy(tag => tag).ToArray();
    }
}
