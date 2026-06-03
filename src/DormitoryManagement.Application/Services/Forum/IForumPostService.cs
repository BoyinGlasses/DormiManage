using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Forum;

namespace DormitoryManagement.Application.Services.Forum;

public interface IForumPostService
{
    Task<PagedResult<ForumPostListItemDto>> GetFeedAsync(ForumPostFilterRequest request, CancellationToken ct = default);
    Task<Result<ForumPostDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<ForumPostDto>> CreateAsync(CreateForumPostRequest request, CancellationToken ct = default);
    Task<Result<ForumPostDto>> UpdateAsync(UpdateForumPostRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Result> HideAsync(Guid id, CancellationToken ct = default);
}
