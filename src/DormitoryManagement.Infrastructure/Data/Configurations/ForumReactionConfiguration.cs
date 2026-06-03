using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class ForumReactionConfiguration : IEntityTypeConfiguration<ForumReaction>
{
    public void Configure(EntityTypeBuilder<ForumReaction> builder)
    {
        builder.ToTable("forum_reactions", table =>
            table.HasCheckConstraint(
                "CK_forum_reactions_target",
                "([ForumPostId] IS NOT NULL AND [ForumCommentId] IS NULL) OR ([ForumPostId] IS NULL AND [ForumCommentId] IS NOT NULL)"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReactionType).HasMaxLength(30).IsRequired();
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ForumPost)
            .WithMany(x => x.Reactions)
            .HasForeignKey(x => x.ForumPostId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ForumComment)
            .WithMany(x => x.Reactions)
            .HasForeignKey(x => x.ForumCommentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.UserId, x.ForumPostId })
            .IsUnique()
            .HasFilter("[ForumPostId] IS NOT NULL");
        builder.HasIndex(x => new { x.UserId, x.ForumCommentId })
            .IsUnique()
            .HasFilter("[ForumCommentId] IS NOT NULL");
    }
}
