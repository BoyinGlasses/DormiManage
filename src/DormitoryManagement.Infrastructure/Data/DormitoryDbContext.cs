using DormitoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Data;

public sealed class DormitoryDbContext : DbContext
{
    public DormitoryDbContext(DbContextOptions<DormitoryDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PendingAccountRegistration> PendingAccountRegistrations => Set<PendingAccountRegistration>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomRegistration> RoomRegistrations => Set<RoomRegistration>();
    public DbSet<RoomAssignment> RoomAssignments => Set<RoomAssignment>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<FeeType> FeeTypes => Set<FeeType>();
    public DbSet<FeeRate> FeeRates => Set<FeeRate>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<InvoiceAdjustment> InvoiceAdjustments => Set<InvoiceAdjustment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<PaymentExtension> PaymentExtensions => Set<PaymentExtension>();
    public DbSet<UtilityReading> UtilityReadings => Set<UtilityReading>();
    public DbSet<VehicleRegistration> VehicleRegistrations => Set<VehicleRegistration>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportTicketResponse> SupportTicketResponses => Set<SupportTicketResponse>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ForumCategory> ForumCategories => Set<ForumCategory>();
    public DbSet<ForumTag> ForumTags => Set<ForumTag>();
    public DbSet<ForumPost> ForumPosts => Set<ForumPost>();
    public DbSet<ForumPostTag> ForumPostTags => Set<ForumPostTag>();
    public DbSet<ForumComment> ForumComments => Set<ForumComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DormitoryDbContext).Assembly);
        SeedData.Apply(modelBuilder);
    }
}
