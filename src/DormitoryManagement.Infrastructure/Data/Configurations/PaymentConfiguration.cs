using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>, IEntityTypeConfiguration<PaymentAllocation>, IEntityTypeConfiguration<PaymentExtension>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PaymentCode).HasColumnName("payment_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Method).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.TransactionRef).HasColumnName("transaction_ref").HasMaxLength(100);
        builder.HasIndex(x => x.PaymentCode).IsUnique();
        builder.HasIndex(x => x.TransactionRef).IsUnique().HasFilter("[transaction_ref] IS NOT NULL");
        builder.HasOne(x => x.Student).WithMany(x => x.Payments).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TargetInvoice).WithMany().HasForeignKey(x => x.TargetInvoiceId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable("payment_allocations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.HasOne(x => x.Payment).WithMany(x => x.Allocations).HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Invoice).WithMany(x => x.PaymentAllocations).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<PaymentExtension> builder)
    {
        builder.ToTable("payment_extensions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.RejectionReason).HasMaxLength(500);
        builder.HasIndex(x => new { x.InvoiceId, x.Status });
        builder.HasOne(x => x.Invoice).WithMany(x => x.PaymentExtensions).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Student).WithMany(x => x.PaymentExtensions).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReviewedByUser).WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}
