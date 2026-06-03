using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Forum;

namespace DormitoryManagement.Application.Services.Forum;

public interface IForumCommentService
{
    Task<IReadOnlyList<ForumCommentDto>> GetPostCommentsAsync(Guid postId, CancellationToken ct = default);
    Task<Result<ForumCommentDto>> CreateAsync(CreateForumCommentRequest request, CancellationToken ct = default);
    Task<Result<ForumCommentDto>> UpdateAsync(UpdateForumCommentRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid commentId, CancellationToken ct = default);
}
