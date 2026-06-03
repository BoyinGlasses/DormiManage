namespace DormitoryManagement.Infrastructure.Services;

public sealed class MockPaymentGateway
{
    public Task<string> CreateTransactionAsync(decimal amount, CancellationToken ct = default)
    {
        // TODO: Replace this mock with a real gateway only if a desktop-safe integration is selected.
        return Task.FromResult($"MOCK-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{amount:0}");
    }
}
