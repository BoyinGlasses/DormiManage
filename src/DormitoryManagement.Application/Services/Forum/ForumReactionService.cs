using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Forum;

public sealed class ForumReactionService : IForumReactionService
{
    private const string LikeReactionType = "Like";
    private readonly IForumPostRepository _posts;
    private readonly IForumCommentRepository _comments;
    private readonly IForumReactionRepository _reactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ForumReactionService(
        IForumPostRepository posts,
        IForumCommentRepository comments,
        IForumReactionRepository reactions,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _posts = posts;
        _comments = comments;
        _reactions = reactions;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> TogglePostLikeAsync(Guid postId, CancellationToken ct = default)
    {
        if (!EnsureAuthenticated(out var userId, out var error))
        {
            return Result.Failure(error);
        }

        var post = await _posts.GetByIdWithDetailsAsync(postId, ct);
        if (post is null || post.Status != ForumPostStatus.Published)
        {
            return Result.Failure("Forum post was not found or is not visible.");
        }

        var existing = await _reactions.GetPostLikeAsync(userId, postId, ct);
        if (existing is not null)
        {
            _reactions.Remove(existing);
        }
        else
        {
            await _reactions.AddAsync(new ForumReaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ForumPostId = postId,
                ReactionType = LikeReactionType,
                CreatedAt = DateTime.UtcNow
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ToggleCommentLikeAsync(Guid commentId, CancellationToken ct = default)
    {
        if (!EnsureAuthenticated(out var userId, out var error))
        {
            return Result.Failure(error);
        }

        var comment = await _comments.GetByIdWithDetailsAsync(commentId, ct);
        if (comment is null
            || comment.Status != ForumCommentStatus.Published
            || comment.ForumPost is null
            || comment.ForumPost.Status != ForumPostStatus.Published)
        {
            return Result.Failure("Forum comment was not found or is not visible.");
        }

        var existing = await _reactions.GetCommentLikeAsync(userId, commentId, ct);
        if (existing is not null)
        {
            _reactions.Remove(existing);
        }
        else
        {
            await _reactions.AddAsync(new ForumReaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ForumCommentId = commentId,
                ReactionType = LikeReactionType,
                CreatedAt = DateTime.UtcNow
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
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
}
