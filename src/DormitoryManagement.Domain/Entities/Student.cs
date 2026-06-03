using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class Student : SoftDeleteEntity
{
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Department { get; set; }
    public string? ClassName { get; set; }
    public DateTime? EnrollmentDate { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.NotRegistered;
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public Guid? CurrentRoomId { get; set; }
    public Room? CurrentRoom { get; set; }
    public ICollection<RoomRegistration> RoomRegistrations { get; set; } = new List<RoomRegistration>();
    public ICollection<RoomAssignment> RoomAssignments { get; set; } = new List<RoomAssignment>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<PaymentExtension> PaymentExtensions { get; set; } = new List<PaymentExtension>();
    public ICollection<VehicleRegistration> VehicleRegistrations { get; set; } = new List<VehicleRegistration>();
    public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
}
