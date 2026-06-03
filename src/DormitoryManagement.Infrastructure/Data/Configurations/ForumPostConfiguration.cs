using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class ForumPostConfiguration : IEntityTypeConfiguration<ForumPost>
{
    public void Configure(EntityTypeBuilder<ForumPost> builder)
    {
        builder.ToTable("forum_posts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.Excerpt).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Area).HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasOne(x => x.AuthorUser).WithMany().HasForeignKey(x => x.AuthorUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Tags).WithOne(x => x.ForumPost).HasForeignKey(x => x.ForumPostId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.Area);
    }
}
