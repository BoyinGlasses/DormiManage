namespace DormitoryManagement.Application.DTOs.Settings;

public sealed class FeeTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public bool IsRecurring { get; set; }
    public string Status { get; set; } = "Active";
}
