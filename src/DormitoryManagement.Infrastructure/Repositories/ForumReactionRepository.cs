using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Repositories;

public sealed class ForumReactionRepository : IForumReactionRepository
{
    private readonly DormitoryDbContext _dbContext;

    public ForumReactionRepository(DormitoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ForumReaction>> GetForPostAsync(Guid postId, CancellationToken ct = default) =>
        await _dbContext.ForumReactions
            .AsNoTracking()
            .Where(reaction => reaction.ForumPostId == postId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ForumReaction>> GetForCommentsAsync(IReadOnlyList<Guid> commentIds, CancellationToken ct = default) =>
        await _dbContext.ForumReactions
            .AsNoTracking()
            .Where(reaction => reaction.ForumCommentId.HasValue && commentIds.Contains(reaction.ForumCommentId.Value))
            .ToListAsync(ct);

    public Task<ForumReaction?> GetPostLikeAsync(Guid userId, Guid postId, CancellationToken ct = default) =>
        _dbContext.ForumReactions.FirstOrDefaultAsync(reaction => reaction.UserId == userId && reaction.ForumPostId == postId, ct);

    public Task<ForumReaction?> GetCommentLikeAsync(Guid userId, Guid commentId, CancellationToken ct = default) =>
        _dbContext.ForumReactions.FirstOrDefaultAsync(reaction => reaction.UserId == userId && reaction.ForumCommentId == commentId, ct);

    public Task AddAsync(ForumReaction reaction, CancellationToken ct = default) =>
        _dbContext.ForumReactions.AddAsync(reaction, ct).AsTask();

    public void Remove(ForumReaction reaction) => _dbContext.ForumReactions.Remove(reaction);
}
