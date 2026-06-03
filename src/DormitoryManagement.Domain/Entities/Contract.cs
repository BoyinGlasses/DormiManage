using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class Contract : AuditableEntity
{
    public string ContractNumber { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MonthlyFee { get; set; }
    public decimal DepositAmount { get; set; }
    public int TermMonths { get; set; } = 12;
    public decimal TotalAmount { get; set; }
    public bool IncludesInternet { get; set; }
    public Guid? RoomRegistrationId { get; set; }
    public RoomRegistration? RoomRegistration { get; set; }
    public Guid? UpfrontInvoiceId { get; set; }
    public Invoice? UpfrontInvoice { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Active;
}
