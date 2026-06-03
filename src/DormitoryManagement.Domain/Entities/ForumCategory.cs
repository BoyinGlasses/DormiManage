using DormitoryManagement.Domain.Common;

namespace DormitoryManagement.Domain.Entities;

public class ForumCategory : SoftDeleteEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ForumPost> Posts { get; set; } = new List<ForumPost>();
}
