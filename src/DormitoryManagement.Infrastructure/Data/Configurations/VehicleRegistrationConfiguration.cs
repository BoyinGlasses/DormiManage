using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class VehicleRegistrationConfiguration : IEntityTypeConfiguration<VehicleRegistration>
{
    public void Configure(EntityTypeBuilder<VehicleRegistration> builder)
    {
        builder.ToTable("vehicle_registrations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LicensePlate).HasColumnName("license_plate").HasMaxLength(20).IsRequired();
        builder.Property(x => x.VehicleType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MonthCount).HasColumnName("month_count");
        builder.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.InvoiceId).HasColumnName("invoice_id");
        builder.Property(x => x.PaymentDate).HasColumnName("payment_date");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.LicensePlate);
        builder.HasOne(x => x.Student).WithMany(x => x.VehicleRegistrations).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.SetNull);
    }
}
