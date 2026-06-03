using DormitoryManagement.Domain.Common;

namespace DormitoryManagement.Domain.Entities;

public class ForumTag : SoftDeleteEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<ForumPostTag> PostTags { get; set; } = new List<ForumPostTag>();
}
