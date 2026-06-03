using DormitoryManagement.Infrastructure.Data;
using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Tests;

public sealed class DormitoryDbContextTests
{
    [Fact]
    public void DbContext_Model_CanBeCreated()
    {
        var options = new DbContextOptionsBuilder<DormitoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new DormitoryDbContext(options);

        Assert.NotNull(dbContext.Model.FindEntityType(typeof(DormitoryManagement.Domain.Entities.Student)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(DormitoryManagement.Domain.Entities.PendingAccountRegistration)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(PaymentExtension)));
        var invoice = dbContext.Model.FindEntityType(typeof(Invoice));
        Assert.NotNull(invoice?.FindProperty(nameof(Invoice.InvoiceKind)));
        Assert.NotNull(invoice?.FindProperty(nameof(Invoice.ContractId)));
        Assert.NotNull(invoice?.FindProperty(nameof(Invoice.TransferContent)));
        Assert.NotNull(invoice?.FindProperty(nameof(Invoice.QrDataUrl)));
        Assert.NotNull(invoice?.FindProperty(nameof(Invoice.BankTransactionId)));
        Assert.Contains(invoice!.GetIndexes(), index =>
            index.IsUnique &&
            index.GetFilter() == "[transfer_content] IS NOT NULL" &&
            index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(Invoice.TransferContent) }));
        Assert.Contains(invoice.GetIndexes(), index =>
            index.IsUnique &&
            index.GetFilter() == "[bank_transaction_id] IS NOT NULL" &&
            index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(Invoice.BankTransactionId) }));
        var invoicePeriodIndex = Assert.Single(invoice!.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual(new[]
            {
                nameof(Invoice.StudentId),
                nameof(Invoice.RoomId),
                nameof(Invoice.BillingPeriod),
                nameof(Invoice.InvoiceKind)
            }));
        Assert.True(invoicePeriodIndex.IsUnique);
        Assert.Equal("[InvoiceKind] = 'MonthlyUtility'", invoicePeriodIndex.GetFilter());
    }
}
