using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Repositories;

public sealed class ForumCommentRepository : IForumCommentRepository
{
    private readonly DormitoryDbContext _dbContext;

    public ForumCommentRepository(DormitoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ForumComment>> GetPostCommentsWithDetailsAsync(Guid postId, CancellationToken ct = default) =>
        await _dbContext.ForumComments
            .AsNoTracking()
            .Include(comment => comment.AuthorUser)
            .ThenInclude(user => user!.Role)
            .Where(comment => comment.ForumPostId == postId && comment.Status == ForumCommentStatus.Published)
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync(ct);

    public Task<ForumComment?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.ForumComments
            .Include(comment => comment.AuthorUser)
            .ThenInclude(user => user!.Role)
            .Include(comment => comment.ForumPost)
            .FirstOrDefaultAsync(comment => comment.Id == id, ct);

    public Task AddAsync(ForumComment comment, CancellationToken ct = default) =>
        _dbContext.ForumComments.AddAsync(comment, ct).AsTask();

    public void Update(ForumComment comment) => _dbContext.ForumComments.Update(comment);
}
