using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class CreateForumCommentRequest
{
    [Required]
    public Guid PostId { get; set; }

    public Guid? ParentCommentId { get; set; }

    [Required, StringLength(3000)]
    public string Content { get; set; } = string.Empty;
}
