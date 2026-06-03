using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class ForumConfiguration :
    IEntityTypeConfiguration<ForumCategory>,
    IEntityTypeConfiguration<ForumTag>,
    IEntityTypeConfiguration<ForumPost>,
    IEntityTypeConfiguration<ForumPostTag>,
    IEntityTypeConfiguration<ForumComment>
{
    public void Configure(EntityTypeBuilder<ForumCategory> builder)
    {
        builder.ToTable("forum_categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasIndex(x => x.Code).IsUnique();
    }

    public void Configure(EntityTypeBuilder<ForumTag> builder)
    {
        builder.ToTable("forum_tags");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
    }

    public void Configure(EntityTypeBuilder<ForumPost> builder)
    {
        builder.ToTable("forum_posts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.VisibilityScope).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.TargetRoleName).HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasOne(x => x.Category).WithMany(x => x.Posts).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Building).WithMany().HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.CreatedByUserId);
        builder.HasIndex(x => new { x.VisibilityScope, x.BuildingId, x.RoomId, x.TargetRoleName });
    }

    public void Configure(EntityTypeBuilder<ForumPostTag> builder)
    {
        builder.ToTable("forum_post_tags");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.PostId, x.TagId }).IsUnique();
        builder.HasOne(x => x.Post).WithMany(x => x.PostTags).HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Tag).WithMany(x => x.PostTags).HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<ForumComment> builder)
    {
        builder.ToTable("forum_comments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).HasMaxLength(3000).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasOne(x => x.Post).WithMany(x => x.Comments).HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ParentComment).WithMany(x => x.Replies).HasForeignKey(x => x.ParentCommentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.PostId);
        builder.HasIndex(x => x.CreatedByUserId);
    }
}
