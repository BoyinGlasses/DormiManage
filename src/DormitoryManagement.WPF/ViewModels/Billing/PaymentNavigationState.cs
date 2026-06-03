using DormitoryManagement.Application.DTOs.Payments;

namespace DormitoryManagement.WPF.ViewModels.Billing;

public sealed class PaymentNavigationState
{
    public PaymentNavigationContextDto? PaymentContext { get; private set; }
    public PaymentNavigationContextDto? ExtensionContext { get; private set; }

    public void SetPaymentContext(PaymentNavigationContextDto context)
    {
        PaymentContext = context;
        ExtensionContext = null;
    }

    public void SetExtensionContext(PaymentNavigationContextDto context)
    {
        ExtensionContext = context;
        PaymentContext = null;
    }

    public void Clear()
    {
        PaymentContext = null;
        ExtensionContext = null;
    }
}
