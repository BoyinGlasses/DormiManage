using DormitoryManagement.Domain.Entities;

namespace DormitoryManagement.Application.Abstractions.Repositories;

public interface IForumReactionRepository
{
    Task<IReadOnlyList<ForumReaction>> GetForPostAsync(Guid postId, CancellationToken ct = default);
    Task<IReadOnlyList<ForumReaction>> GetForCommentsAsync(IReadOnlyList<Guid> commentIds, CancellationToken ct = default);
    Task<ForumReaction?> GetPostLikeAsync(Guid userId, Guid postId, CancellationToken ct = default);
    Task<ForumReaction?> GetCommentLikeAsync(Guid userId, Guid commentId, CancellationToken ct = default);
    Task AddAsync(ForumReaction reaction, CancellationToken ct = default);
    void Remove(ForumReaction reaction);
}
