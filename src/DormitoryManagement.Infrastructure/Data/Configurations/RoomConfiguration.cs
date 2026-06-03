using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Building>, IEntityTypeConfiguration<Floor>, IEntityTypeConfiguration<Room>, IEntityTypeConfiguration<RoomAssignment>, IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("buildings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }

    public void Configure(EntityTypeBuilder<Floor> builder)
    {
        builder.ToTable("floors");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.BuildingId, x.FloorNumber }).IsUnique();
        builder.HasOne(x => x.Building).WithMany(x => x.Floors).HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RoomNumber).HasColumnName("room_number").HasMaxLength(20).IsRequired();
        builder.Property(x => x.MonthlyPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.GenderType).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => new { x.BuildingId, x.FloorId, x.RoomNumber }).IsUnique();
        builder.HasOne(x => x.Building).WithMany(x => x.Rooms).HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Floor).WithMany(x => x.Rooms).HasForeignKey(x => x.FloorId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<RoomAssignment> builder)
    {
        builder.ToTable("room_assignments");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.StudentId).IsUnique().HasFilter("[IsActive] = 1");
        builder.HasOne(x => x.Student).WithMany(x => x.RoomAssignments).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Room).WithMany(x => x.Assignments).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contracts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ContractNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MonthlyFee).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DepositAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TermMonths).HasDefaultValue(12);
        builder.Property(x => x.IncludesInternet).HasDefaultValue(false);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.ContractNumber).IsUnique();
        builder.HasOne(x => x.Student).WithMany(x => x.Contracts).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Room).WithMany(x => x.Contracts).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RoomRegistration).WithMany().HasForeignKey(x => x.RoomRegistrationId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.UpfrontInvoice).WithOne().HasForeignKey<Contract>(x => x.UpfrontInvoiceId).OnDelete(DeleteBehavior.SetNull);
    }
}
