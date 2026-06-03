using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class FeeType : SoftDeleteEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public bool IsRecurring { get; set; }
    public ICollection<FeeRate> FeeRates { get; set; } = new List<FeeRate>();
}
