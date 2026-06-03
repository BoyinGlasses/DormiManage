using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StudentCode).HasColumnName("student_code").HasMaxLength(30).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.StudentCode).IsUnique();
        builder.HasIndex(x => x.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
        builder.HasOne(x => x.User).WithOne(x => x.Student).HasForeignKey<Student>(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.CurrentRoom).WithMany().HasForeignKey(x => x.CurrentRoomId).OnDelete(DeleteBehavior.SetNull);
    }
}
