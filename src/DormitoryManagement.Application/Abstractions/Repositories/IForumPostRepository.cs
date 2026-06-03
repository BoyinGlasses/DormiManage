using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Forum;
using DormitoryManagement.Domain.Entities;

namespace DormitoryManagement.Application.Abstractions.Repositories;

public interface IForumPostRepository
{
    IQueryable<ForumPost> Query();
    Task<ForumPost?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ForumPost>> GetFeedAsync(ForumPostFilterRequest filter, CancellationToken ct = default);
    Task AddAsync(ForumPost post, CancellationToken ct = default);
    void Update(ForumPost post);
}
