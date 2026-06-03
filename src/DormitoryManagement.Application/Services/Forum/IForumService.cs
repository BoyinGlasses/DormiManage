using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Forum;

namespace DormitoryManagement.Application.Services.Forum;

public interface IForumService
{
    Task<IReadOnlyList<ForumCategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ForumTagDto>> GetTagsAsync(CancellationToken ct = default);
    Task<PagedResult<ForumPostDto>> GetPostsAsync(ForumPostFilterRequest? request = null, CancellationToken ct = default);
    Task<ForumPostDto?> GetPostAsync(Guid postId, CancellationToken ct = default);
    Task<ForumPostDto> CreatePostAsync(CreateForumPostRequest request, CancellationToken ct = default);
    Task<ForumPostDto> UpdatePostAsync(UpdateForumPostRequest request, CancellationToken ct = default);
    Task DeletePostAsync(Guid postId, CancellationToken ct = default);
    Task<ForumCommentDto> CreateCommentAsync(CreateForumCommentRequest request, CancellationToken ct = default);
    Task DeleteCommentAsync(Guid commentId, CancellationToken ct = default);
}
