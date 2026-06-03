namespace DormitoryManagement.Application.DTOs.Billing;

public sealed class InvoicePaymentQrDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public decimal Amount { get; set; }
    public string TransferContent { get; set; } = string.Empty;
    public string QrDataUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
}
