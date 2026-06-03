using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<FeeType>, IEntityTypeConfiguration<FeeRate>, IEntityTypeConfiguration<Invoice>, IEntityTypeConfiguration<InvoiceItem>, IEntityTypeConfiguration<InvoiceAdjustment>, IEntityTypeConfiguration<UtilityReading>
{
    public void Configure(EntityTypeBuilder<FeeType> builder)
    {
        builder.ToTable("fee_types");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }

    public void Configure(EntityTypeBuilder<FeeRate> builder)
    {
        builder.ToTable("fee_rates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.HasOne(x => x.FeeType).WithMany(x => x.FeeRates).HasForeignKey(x => x.FeeTypeId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InvoiceNumber).HasColumnName("invoice_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.BillingPeriod).HasColumnName("billing_period").HasMaxLength(7).IsRequired();
        builder.Property(x => x.InvoiceKind).HasConversion<string>().HasMaxLength(30).HasDefaultValue(DormitoryManagement.Domain.Enums.InvoiceKind.MonthlyUtility);
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.TransferContent).HasColumnName("transfer_content").HasMaxLength(200);
        builder.Property(x => x.QrDataUrl).HasColumnName("qr_data_url").HasMaxLength(2000);
        builder.Property(x => x.BankTransactionId).HasColumnName("bank_transaction_id").HasMaxLength(100);
        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
        builder.HasIndex(x => x.TransferContent).IsUnique().HasFilter("[transfer_content] IS NOT NULL");
        builder.HasIndex(x => x.BankTransactionId).IsUnique().HasFilter("[bank_transaction_id] IS NOT NULL");
        builder.HasIndex(x => new { x.StudentId, x.RoomId, x.BillingPeriod, x.InvoiceKind })
            .IsUnique()
            .HasFilter("[InvoiceKind] = 'MonthlyUtility'");
        builder.HasOne(x => x.Student).WithMany(x => x.Invoices).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Room).WithMany(x => x.Invoices).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Contract).WithMany().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.SetNull);
    }

    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.HasOne(x => x.Invoice).WithMany(x => x.Items).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.FeeType).WithMany().HasForeignKey(x => x.FeeTypeId).OnDelete(DeleteBehavior.SetNull);
    }

    public void Configure(EntityTypeBuilder<InvoiceAdjustment> builder)
    {
        builder.ToTable("invoice_adjustments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.HasOne(x => x.Invoice).WithMany(x => x.Adjustments).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
    }

    public void Configure(EntityTypeBuilder<UtilityReading> builder)
    {
        builder.ToTable("utility_readings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BillingPeriod).HasMaxLength(7).IsRequired();
        builder.Property(x => x.ElectricityPrevious).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ElectricityCurrent).HasColumnType("decimal(18,2)");
        builder.Property(x => x.WaterPrevious).HasColumnType("decimal(18,2)");
        builder.Property(x => x.WaterCurrent).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => new { x.RoomId, x.BillingPeriod }).IsUnique();
        builder.HasOne(x => x.Room).WithMany(x => x.UtilityReadings).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
    }
}
