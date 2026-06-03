using DormitoryManagement.Domain.Entities;

namespace DormitoryManagement.Application.Abstractions.Repositories;

public interface IForumCommentRepository
{
    Task<IReadOnlyList<ForumComment>> GetPostCommentsWithDetailsAsync(Guid postId, CancellationToken ct = default);
    Task<ForumComment?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ForumComment comment, CancellationToken ct = default);
    void Update(ForumComment comment);
}
