using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DormitoryManagement.Infrastructure.Data.Configurations;

public sealed class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>, IEntityTypeConfiguration<SupportTicketResponse>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.ToTable("support_tickets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(30);
        builder.HasOne(x => x.Student).WithMany(x => x.SupportTickets).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.CreatedByUser).WithMany(x => x.CreatedTickets).HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssignedToManager).WithMany(x => x.AssignedTickets).HasForeignKey(x => x.AssignedToManagerId).OnDelete(DeleteBehavior.SetNull);
    }

    public void Configure(EntityTypeBuilder<SupportTicketResponse> builder)
    {
        builder.ToTable("support_ticket_responses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Message).HasMaxLength(4000).IsRequired();
        builder.HasOne(x => x.SupportTicket).WithMany(x => x.Responses).HasForeignKey(x => x.SupportTicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany(x => x.TicketResponses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
