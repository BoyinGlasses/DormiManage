using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class PendingAccountRegistrationConfiguration : IEntityTypeConfiguration<PendingAccountRegistration>
{
    public void Configure(EntityTypeBuilder<PendingAccountRegistration> builder)
    {
        builder.ToTable("pending_account_registrations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FullName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Username).HasMaxLength(50).IsRequired();
        builder.Property(x => x.StudentCode).HasColumnName("student_code").HasMaxLength(20).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.Gender).HasMaxLength(20);
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
        builder.Property(x => x.OtpHash).HasColumnName("otp_hash").HasMaxLength(500).IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(x => x.LastSentAt).HasColumnName("last_sent_at").IsRequired();
        builder.Property(x => x.AttemptCount).HasColumnName("attempt_count").IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Username).IsUnique();
        builder.HasIndex(x => x.StudentCode).IsUnique();
    }
}
