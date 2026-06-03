using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class RoomRegistrationConfiguration : IEntityTypeConfiguration<RoomRegistration>
{
    public void Configure(EntityTypeBuilder<RoomRegistration> builder)
    {
        builder.ToTable("room_registrations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.ContractTermMonths).HasDefaultValue(12);
        builder.Property(x => x.IncludesInternet).HasDefaultValue(false);
        builder.HasOne(x => x.Student).WithMany(x => x.RoomRegistrations).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Room).WithMany(x => x.Registrations).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReviewedByUser).WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}
