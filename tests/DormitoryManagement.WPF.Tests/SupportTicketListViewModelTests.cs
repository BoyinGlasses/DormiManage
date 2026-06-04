using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.DTOs.SupportTickets;
using DormitoryManagement.Application.Services.SupportTickets;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.ViewModels.SupportTickets;

namespace DormitoryManagement.WPF.Tests;

public sealed class SupportTicketListViewModelTests
{
    [Fact]
    public async Task Load_updates_summary_counts_from_all_ticket_states()
    {
        var service = new StubSupportTicketService();
        var viewModel = new SupportTicketListViewModel(service, new StubCurrentUser(RoleNames.Student));

        viewModel.LoadCommand.Execute(null);
        await WaitUntilAsync(() => viewModel.HasTickets);

        Assert.Equal(4, viewModel.TotalTicketCount);
        Assert.Equal(3, viewModel.OpenTicketCount);
        Assert.Equal(1, viewModel.ResolvedTicketCount);
        Assert.Equal("4", viewModel.TotalTicketCountText);
        Assert.Equal("3", viewModel.OpenTicketCountText);
        Assert.Equal("1", viewModel.ResolvedTicketCountText);
    }

    [Fact]
    public async Task Create_ticket_keeps_form_open_on_validation_and_closes_after_success()
    {
        var service = new StubSupportTicketService();
        var viewModel = new SupportTicketListViewModel(service, new StubCurrentUser(RoleNames.Student));

        viewModel.ToggleCreateFormCommand.Execute(null);
        Assert.True(viewModel.IsCreateFormOpen);

        viewModel.CreateTicketCommand.Execute(null);
        await Task.Delay(50);

        Assert.True(viewModel.IsCreateFormOpen);
        Assert.Equal("Enter a ticket title.", viewModel.TitleError);
        Assert.Equal("Enter a ticket description.", viewModel.DescriptionError);

        viewModel.Title = "Need desk repair";
        viewModel.Description = "The desk hinge is loose.";
        viewModel.CreateTicketCommand.Execute(null);
        await WaitUntilAsync(() => viewModel.HasSuccessMessage);

        Assert.False(viewModel.IsCreateFormOpen);
        Assert.Equal("Support ticket created.", viewModel.SuccessMessage);
        Assert.Equal("Need desk repair", service.CreatedTickets[0].Title);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition())
        {
            if (DateTime.UtcNow >= deadline)
            {
                throw new Xunit.Sdk.XunitException("Timed out waiting for expected support-ticket state.");
            }

            await Task.Delay(20);
        }
    }

    private sealed class StubCurrentUser : ICurrentUserService
    {
        private readonly string _roleName;

        public StubCurrentUser(string roleName)
        {
            _roleName = roleName;
            CurrentUser = new CurrentUserDto
            {
                UserId = Guid.NewGuid(),
                Username = "student",
                Email = "student@ktx.local",
                FullName = "Nguyễn Văn A",
                RoleName = roleName,
                StudentId = Guid.NewGuid()
            };
        }

        public CurrentUserDto? CurrentUser { get; }
        public Guid? UserId => CurrentUser?.UserId;
        public string? UserName => CurrentUser?.Username;
        public string? Email => CurrentUser?.Email;
        public string? FullName => CurrentUser?.FullName;
        public IReadOnlyCollection<string> Roles => [_roleName];
        public bool IsAuthenticated => true;
        public bool IsInRole(string roleName) => string.Equals(roleName, _roleName, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class StubSupportTicketService : ISupportTicketService
    {
        private readonly List<SupportTicketDto> _tickets =
        [
            new SupportTicketDto { Id = Guid.NewGuid(), Title = "Fix bathroom light", Description = "A", Category = SupportTicketCategory.Maintenance, Priority = PriorityLevel.Medium, Status = SupportTicketStatus.New, CreatedAt = new DateTime(2026, 6, 1) },
            new SupportTicketDto { Id = Guid.NewGuid(), Title = "Wifi issue", Description = "B", Category = SupportTicketCategory.Account, Priority = PriorityLevel.High, Status = SupportTicketStatus.Assigned, CreatedAt = new DateTime(2026, 6, 2) },
            new SupportTicketDto { Id = Guid.NewGuid(), Title = "Billing question", Description = "C", Category = SupportTicketCategory.Billing, Priority = PriorityLevel.Low, Status = SupportTicketStatus.InProgress, CreatedAt = new DateTime(2026, 6, 3) },
            new SupportTicketDto { Id = Guid.NewGuid(), Title = "Broken lock", Description = "D", Category = SupportTicketCategory.Security, Priority = PriorityLevel.High, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2026, 5, 29) }
        ];

        public List<SupportTicketDto> CreatedTickets { get; } = new();

        public Task<IReadOnlyList<SupportTicketDto>> GetTicketsAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<SupportTicketDto>>(_tickets.ToArray());

        public Task<SupportTicketDto?> GetTicketAsync(Guid ticketId, CancellationToken ct = default) =>
            Task.FromResult(_tickets.FirstOrDefault(ticket => ticket.Id == ticketId));

        public Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketRequest request, CancellationToken ct = default)
        {
            var created = new SupportTicketDto
            {
                Id = Guid.NewGuid(),
                StudentId = request.StudentId,
                Title = request.Title,
                Description = request.Description,
                Category = request.Category,
                Priority = request.Priority,
                Status = SupportTicketStatus.New,
                CreatedAt = new DateTime(2026, 6, 4)
            };
            CreatedTickets.Insert(0, created);
            _tickets.Insert(0, created);
            return Task.FromResult(created);
        }

        public Task AssignTicketAsync(Guid ticketId, Guid managerId, CancellationToken ct = default) => Task.CompletedTask;
        public Task AddResponseAsync(Guid ticketId, string message, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateStatusAsync(UpdateSupportTicketStatusRequest request, CancellationToken ct = default) => Task.CompletedTask;
        public Task CloseTicketAsync(Guid ticketId, CancellationToken ct = default) => Task.CompletedTask;
    }
}

