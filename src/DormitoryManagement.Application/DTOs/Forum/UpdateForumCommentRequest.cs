namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class UpdateForumCommentRequest
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
}
