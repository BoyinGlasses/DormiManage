namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class ForumPostDto : ForumPostListItemDto
{
    public string Content { get; set; } = string.Empty;
}
