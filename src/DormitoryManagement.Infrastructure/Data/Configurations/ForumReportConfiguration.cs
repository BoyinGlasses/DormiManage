using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class ForumReportConfiguration : IEntityTypeConfiguration<ForumReport>
{
    public void Configure(EntityTypeBuilder<ForumReport> builder)
    {
        builder.ToTable("forum_reports");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TargetType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Details).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.ResolutionAction).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.ResolutionNote).HasMaxLength(1000);
        builder.HasOne(x => x.ReporterUser).WithMany().HasForeignKey(x => x.ReporterUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReviewedByUser).WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ForumPost).WithMany().HasForeignKey(x => x.ForumPostId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ForumComment).WithMany().HasForeignKey(x => x.ForumCommentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
        builder.HasIndex(x => new { x.ReporterUserId, x.ForumPostId, x.Status });
        builder.HasIndex(x => new { x.ReporterUserId, x.ForumCommentId, x.Status });
    }
}
