namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class BankTransferProcessResultDto
{
    public bool Matched { get; set; }
    public bool Duplicate { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? PaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
