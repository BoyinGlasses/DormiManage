using DormitoryManagement.Application.Common;

namespace DormitoryManagement.Application.Services.Forum;

public interface IForumReactionService
{
    Task<Result> TogglePostLikeAsync(Guid postId, CancellationToken ct = default);
    Task<Result> ToggleCommentLikeAsync(Guid commentId, CancellationToken ct = default);
}
