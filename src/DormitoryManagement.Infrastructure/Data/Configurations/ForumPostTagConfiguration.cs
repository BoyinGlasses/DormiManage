using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class ForumPostTagConfiguration : IEntityTypeConfiguration<ForumPostTag>
{
    public void Configure(EntityTypeBuilder<ForumPostTag> builder)
    {
        builder.ToTable("forum_post_tags");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.ForumPostId, x.Name }).IsUnique();
    }
}
