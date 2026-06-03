using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.DTOs.Dashboard;
using DormitoryManagement.Application.Services.Dashboard;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Tests;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task GetStudentDashboardAsync_returns_empty_room_card_when_student_has_no_assignment_or_active_request()
    {
        var fixture = DashboardFixture.Create();

        var dashboard = await fixture.Service.GetStudentDashboardAsync(fixture.Student.Id);

        Assert.Null(dashboard.CurrentRoom);
        Assert.Null(dashboard.RequestedRoom);
        Assert.Equal("Empty", dashboard.RoomCardDisplayMode);
        Assert.Equal("Chưa phân phòng", dashboard.RoomCardStatusText);
        Assert.True(dashboard.CanOpenRoomRegistrationPopup);
        Assert.Null(dashboard.RoomCardLockReason);
    }

    [Fact]
    public async Task GetStudentDashboardAsync_returns_pending_room_card_from_latest_pending_registration()
    {
        var fixture = DashboardFixture.Create();
        fixture.UnitOfWork.Set<RoomRegistration>().Items.Add(new RoomRegistration
        {
            Id = Guid.NewGuid(),
            StudentId = fixture.Student.Id,
            RoomId = fixture.Room.Id,
            Status = RegistrationStatus.Pending,
            RequestedAt = DateTime.UtcNow
        });

        var dashboard = await fixture.Service.GetStudentDashboardAsync(fixture.Student.Id);

        Assert.Null(dashboard.CurrentRoom);
        Assert.Equal("A-101", dashboard.RequestedRoom);
        Assert.Equal("Pending", dashboard.RoomCardDisplayMode);
        Assert.Equal("(đang chờ xử lí)", dashboard.RoomCardStatusText);
        Assert.False(dashboard.CanOpenRoomRegistrationPopup);
        Assert.Equal("Yêu cầu đăng ký đang chờ xử lí.", dashboard.RoomCardLockReason);
    }

    [Fact]
    public async Task GetStudentDashboardAsync_treats_payment_pending_registration_as_locked_pending_state()
    {
        var fixture = DashboardFixture.Create();
        fixture.UnitOfWork.Set<RoomRegistration>().Items.Add(new RoomRegistration
        {
            Id = Guid.NewGuid(),
            StudentId = fixture.Student.Id,
            RoomId = fixture.Room.Id,
            Status = RegistrationStatus.PaymentPending,
            RequestedAt = DateTime.UtcNow
        });

        var dashboard = await fixture.Service.GetStudentDashboardAsync(fixture.Student.Id);

        Assert.Equal("Pending", dashboard.RoomCardDisplayMode);
        Assert.Equal("A-101", dashboard.RequestedRoom);
        Assert.False(dashboard.CanOpenRoomRegistrationPopup);
        Assert.Equal("Yêu cầu đăng ký đang chờ thanh toán hợp đồng.", dashboard.RoomCardLockReason);
    }

    [Fact]
    public async Task GetStudentDashboardAsync_prefers_active_assignment_over_pending_registration()
    {
        var fixture = DashboardFixture.Create();
        fixture.UnitOfWork.Set<RoomRegistration>().Items.Add(new RoomRegistration
        {
            Id = Guid.NewGuid(),
            StudentId = fixture.Student.Id,
            RoomId = fixture.Room.Id,
            Status = RegistrationStatus.Pending,
            RequestedAt = DateTime.UtcNow
        });
        fixture.UnitOfWork.Set<RoomAssignment>().Items.Add(new RoomAssignment
        {
            Id = Guid.NewGuid(),
            StudentId = fixture.Student.Id,
            RoomId = fixture.Room.Id,
            IsActive = true,
            StartDate = new DateTime(2026, 6, 1)
        });

        var dashboard = await fixture.Service.GetStudentDashboardAsync(fixture.Student.Id);

        Assert.Equal("Assigned", dashboard.RoomCardDisplayMode);
        Assert.Equal("A-101", dashboard.CurrentRoom);
        Assert.Null(dashboard.RequestedRoom);
        Assert.Equal("Đã phân phòng", dashboard.RoomCardStatusText);
        Assert.False(dashboard.CanOpenRoomRegistrationPopup);
    }

    [Fact]
    public async Task GetStudentDashboardAsync_reopens_card_after_rejected_request_when_student_still_has_no_room()
    {
        var fixture = DashboardFixture.Create();
        fixture.UnitOfWork.Set<RoomRegistration>().Items.Add(new RoomRegistration
        {
            Id = Guid.NewGuid(),
            StudentId = fixture.Student.Id,
            RoomId = fixture.Room.Id,
            Status = RegistrationStatus.Rejected,
            RequestedAt = DateTime.UtcNow
        });

        var dashboard = await fixture.Service.GetStudentDashboardAsync(fixture.Student.Id);

        Assert.Equal("Empty", dashboard.RoomCardDisplayMode);
        Assert.True(dashboard.CanOpenRoomRegistrationPopup);
        Assert.Null(dashboard.RequestedRoom);
        Assert.Equal("Chưa phân phòng", dashboard.RoomCardStatusText);
    }

    [Fact]
    public async Task GetStudentDashboardAsync_reopens_card_after_cancelled_request_when_student_still_has_no_room()
    {
        var fixture = DashboardFixture.Create();
        fixture.UnitOfWork.Set<RoomRegistration>().Items.Add(new RoomRegistration
        {
            Id = Guid.NewGuid(),
            StudentId = fixture.Student.Id,
            RoomId = fixture.Room.Id,
            Status = RegistrationStatus.Cancelled,
            RequestedAt = DateTime.UtcNow
        });

        var dashboard = await fixture.Service.GetStudentDashboardAsync(fixture.Student.Id);

        Assert.Null(dashboard.CurrentRoom);
        Assert.Null(dashboard.RequestedRoom);
        Assert.Equal("Empty", dashboard.RoomCardDisplayMode);
        Assert.Equal("Chưa phân phòng", dashboard.RoomCardStatusText);
        Assert.True(dashboard.CanOpenRoomRegistrationPopup);
        Assert.Null(dashboard.RoomCardLockReason);
    }

    private sealed class DashboardFixture
    {
        private DashboardFixture()
        {
            UnitOfWork = new InMemoryUnitOfWork();
            Student = new Student
            {
                Id = Guid.NewGuid(),
                StudentCode = "SV001",
                FullName = "Nguyen Van An",
                UserId = Guid.NewGuid(),
                Gender = "Male"
            };
            Building = new Building
            {
                Id = Guid.NewGuid(),
                Name = "Khu A",
                Code = "A"
            };
            Room = new Room
            {
                Id = Guid.NewGuid(),
                BuildingId = Building.Id,
                FloorId = Guid.NewGuid(),
                RoomNumber = "101",
                Capacity = 4,
                MonthlyPrice = 750000m,
                CurrentOccupancy = 0,
                Status = RoomStatus.Available,
                GenderType = RoomGenderType.Male
            };
            UnitOfWork.Set<Student>().Items.Add(Student);
            UnitOfWork.Set<Building>().Items.Add(Building);
            UnitOfWork.Set<Room>().Items.Add(Room);
            Service = new DashboardService(
                new AllowAllPermissionService(),
                new TestCurrentUser(RoleNames.Student, studentId: Student.Id, userId: Student.UserId),
                new InMemoryStudentRepository(UnitOfWork),
                new InMemoryRoomRepository(UnitOfWork),
                new InMemoryInvoiceRepository(UnitOfWork),
                new InMemoryPaymentRepository(UnitOfWork),
                new InMemorySupportTicketRepository(UnitOfWork),
                UnitOfWork);
        }

        public InMemoryUnitOfWork UnitOfWork { get; }
        public Student Student { get; }
        public Building Building { get; }
        public Room Room { get; }
        public DashboardService Service { get; }

        public static DashboardFixture Create() => new();
    }

    private sealed class InMemoryInvoiceRepository : IInvoiceRepository
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        public InMemoryInvoiceRepository(InMemoryUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public IQueryable<Invoice> Query() => _unitOfWork.Set<Invoice>().Items.AsQueryable();
        public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(_unitOfWork.Set<Invoice>().Items.FirstOrDefault(item => item.Id == id));
        public Task AddAsync(Invoice invoice, CancellationToken ct = default) { _unitOfWork.Set<Invoice>().Items.Add(invoice); return Task.CompletedTask; }
        public void Update(Invoice invoice) { }
    }

    private sealed class InMemoryPaymentRepository : IPaymentRepository
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        public InMemoryPaymentRepository(InMemoryUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public IQueryable<Payment> Query() => _unitOfWork.Set<Payment>().Items.AsQueryable();
        public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(_unitOfWork.Set<Payment>().Items.FirstOrDefault(item => item.Id == id));
        public Task AddAsync(Payment payment, CancellationToken ct = default) { _unitOfWork.Set<Payment>().Items.Add(payment); return Task.CompletedTask; }
        public void Update(Payment payment) { }
    }

    private sealed class InMemorySupportTicketRepository : ISupportTicketRepository
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        public InMemorySupportTicketRepository(InMemoryUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public IQueryable<SupportTicket> Query() => _unitOfWork.Set<SupportTicket>().Items.AsQueryable();
        public Task<SupportTicket?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(_unitOfWork.Set<SupportTicket>().Items.FirstOrDefault(item => item.Id == id));
        public Task AddAsync(SupportTicket ticket, CancellationToken ct = default) { _unitOfWork.Set<SupportTicket>().Items.Add(ticket); return Task.CompletedTask; }
        public void Update(SupportTicket ticket) { }
    }
}
