using DormitoryManagement.Domain.Common;

namespace DormitoryManagement.Domain.Entities;

public class Role : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
}
