using DormitoryManagement.Domain.Common;

namespace DormitoryManagement.Domain.Entities;

public class ForumPostTag : BaseEntity
{
    public Guid PostId { get; set; }
    public ForumPost? Post { get; set; }
    public Guid TagId { get; set; }
    public ForumTag? Tag { get; set; }
}
