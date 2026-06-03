using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<Role>, IEntityTypeConfiguration<User>, IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(256).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.FailedLoginCount).HasColumnName("failed_login_count");
        builder.Property(x => x.LockedUntil).HasColumnName("locked_until");
        builder.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Username).IsUnique();
        builder.HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Manager> builder)
    {
        builder.ToTable("managers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StaffCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.StaffCode).IsUnique();
        builder.HasOne(x => x.User).WithOne(x => x.Manager).HasForeignKey<Manager>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Building).WithMany(x => x.Managers).HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.SetNull);
    }
}
