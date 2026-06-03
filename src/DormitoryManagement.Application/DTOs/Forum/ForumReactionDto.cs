namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class ForumReactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ForumPostId { get; set; }
    public Guid? ForumCommentId { get; set; }
    public string ReactionType { get; set; } = "Like";
    public DateTime CreatedAt { get; set; }
}
