using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Forum;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Forum;

public sealed class ForumCommentService : IForumCommentService
{
    private readonly IForumPostRepository _posts;
    private readonly IForumCommentRepository _comments;
    private readonly IForumReactionRepository _reactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissions;

    public ForumCommentService(
        IForumPostRepository posts,
        IForumCommentRepository comments,
        IForumReactionRepository reactions,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IPermissionService permissions)
    {
        _posts = posts;
        _comments = comments;
        _reactions = reactions;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _permissions = permissions;
    }

    public async Task<IReadOnlyList<ForumCommentDto>> GetPostCommentsAsync(Guid postId, CancellationToken ct = default)
    {
        var post = await _posts.GetByIdWithDetailsAsync(postId, ct);
        if (post is null || !await CanViewPostAsync(post, ct))
        {
            return Array.Empty<ForumCommentDto>();
        }

        var comments = await _comments.GetPostCommentsWithDetailsAsync(postId, ct);

        if (comments.Count == 0)
        {
            return Array.Empty<ForumCommentDto>();
        }

        var commentIds = comments.Select(comment => comment.Id).ToArray();
        var reactions = await _reactions.GetForCommentsAsync(commentIds, ct);

        var currentUserId = _currentUser.UserId;
        var reactionLookup = reactions
            .GroupBy(reaction => reaction.ForumCommentId!.Value)
            .ToDictionary(group => group.Key, group => group.ToArray());
        var dtoLookup = comments.ToDictionary(
            comment => comment.Id,
            comment => Map(comment, reactionLookup.GetValueOrDefault(comment.Id) ?? Array.Empty<ForumReaction>(), currentUserId));

        foreach (var comment in comments)
        {
            if (comment.ParentCommentId is { } parentId && dtoLookup.TryGetValue(parentId, out var parent))
            {
                parent.Replies = parent.Replies.Concat([dtoLookup[comment.Id]]).ToArray();
            }
        }

        return comments
            .Where(comment => comment.ParentCommentId is null)
            .Select(comment => dtoLookup[comment.Id])
            .ToArray();
    }

    public async Task<Result<ForumCommentDto>> CreateAsync(CreateForumCommentRequest request, CancellationToken ct = default)
    {
        if (!EnsureAuthenticated(out var userId, out var error))
        {
            return Result<ForumCommentDto>.Failure(error);
        }

        if (!await _permissions.HasPermissionAsync(PermissionNames.ForumCreate, ct))
        {
            return Result<ForumCommentDto>.Failure("You do not have permission to comment in the forum.");
        }

        var validation = ValidateContent(request.Content);
        if (validation is not null)
        {
            return Result<ForumCommentDto>.Failure(validation);
        }

        var post = await _posts.GetByIdWithDetailsAsync(request.ForumPostId, ct);
        if (post is null || post.Status != ForumPostStatus.Published || !await CanViewPostAsync(post, ct))
        {
            return Result<ForumCommentDto>.Failure("Forum post was not found or is not visible.");
        }

        if (request.ParentCommentId is { } parentCommentId)
        {
            var parent = await _comments.GetByIdWithDetailsAsync(parentCommentId, ct);
            if (parent is null || parent.ForumPostId != request.ForumPostId || parent.Status != ForumCommentStatus.Published)
            {
                return Result<ForumCommentDto>.Failure("Reply parent comment was not found under this post.");
            }
        }

        var comment = new ForumComment
        {
            Id = Guid.NewGuid(),
            ForumPostId = request.ForumPostId,
            AuthorUserId = userId,
            ParentCommentId = request.ParentCommentId,
            Content = request.Content.Trim(),
            Status = ForumCommentStatus.Published,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserName
        };

        await _comments.AddAsync(comment, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ForumCommentDto>.Success(await GetCommentDtoAsync(comment.Id, ct));
    }

    public async Task<Result<ForumCommentDto>> UpdateAsync(UpdateForumCommentRequest request, CancellationToken ct = default)
    {
        var comment = await _comments.GetByIdWithDetailsAsync(request.Id, ct);
        if (comment is null)
        {
            return Result<ForumCommentDto>.Failure("Forum comment was not found.");
        }

        if (comment.Status == ForumCommentStatus.Deleted)
        {
            return Result<ForumCommentDto>.Failure("Deleted comments cannot be updated.");
        }

        if (!await CanManageCommentAsync(comment, ct))
        {
            return Result<ForumCommentDto>.Failure("You do not have permission to update this comment.");
        }

        var validation = ValidateContent(request.Content);
        if (validation is not null)
        {
            return Result<ForumCommentDto>.Failure(validation);
        }

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;
        comment.UpdatedBy = _currentUser.UserName;
        _comments.Update(comment);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ForumCommentDto>.Success(await GetCommentDtoAsync(comment.Id, ct));
    }

    public async Task<Result> DeleteAsync(Guid commentId, CancellationToken ct = default)
    {
        var comment = await _comments.GetByIdWithDetailsAsync(commentId, ct);
        if (comment is null)
        {
            return Result.Failure("Forum comment was not found.");
        }

        if (!await CanManageCommentAsync(comment, ct))
        {
            return Result.Failure("You do not have permission to delete this comment.");
        }

        comment.Status = ForumCommentStatus.Deleted;
        comment.UpdatedAt = DateTime.UtcNow;
        comment.UpdatedBy = _currentUser.UserName;
        _comments.Update(comment);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<ForumCommentDto> GetCommentDtoAsync(Guid commentId, CancellationToken ct)
    {
        var comment = await _comments.GetByIdWithDetailsAsync(commentId, ct)
            ?? throw new InvalidOperationException("Forum comment was not found after saving.");
        var reactions = await _reactions.GetForCommentsAsync([commentId], ct);
        return Map(comment, reactions, _currentUser.UserId);
    }

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

    private async Task<bool> CanManageCommentAsync(ForumComment comment, CancellationToken ct) =>
        (_currentUser.UserId == comment.AuthorUserId && await _permissions.HasPermissionAsync(PermissionNames.ForumManageOwn, ct)) ||
        await _permissions.HasPermissionAsync(PermissionNames.ForumModerate, ct);

    private async Task<bool> CanViewPostAsync(ForumPost post, CancellationToken ct)
    {
        if (await _permissions.HasPermissionAsync(PermissionNames.ForumModerate, ct))
        {
            return true;
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

    private static string? ValidateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "Comment content is required.";
        }

        return content.Trim().Length > 4000 ? "Comment content must be 4000 characters or fewer." : null;
    }

    private static ForumCommentDto Map(ForumComment comment, IReadOnlyCollection<ForumReaction> reactions, Guid? currentUserId) => new()
    {
        Id = comment.Id,
        ForumPostId = comment.ForumPostId,
        AuthorUserId = comment.AuthorUserId,
        ParentCommentId = comment.ParentCommentId,
        Content = comment.Content,
        AuthorName = comment.AuthorUser?.FullName ?? comment.AuthorUser?.Username ?? "User",
        AuthorRole = comment.AuthorUser?.Role?.Name ?? string.Empty,
        CreatedAt = comment.CreatedAt,
        LikeCount = reactions.Count,
        IsLikedByCurrentUser = currentUserId.HasValue && reactions.Any(reaction => reaction.UserId == currentUserId.Value)
    };
}
