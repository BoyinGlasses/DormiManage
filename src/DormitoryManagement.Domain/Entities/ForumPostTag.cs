using DormitoryManagement.Domain.Common;

namespace DormitoryManagement.Domain.Entities;

public class ForumPostTag : BaseEntity
{
    public Guid ForumPostId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ForumPost? ForumPost { get; set; }
}
