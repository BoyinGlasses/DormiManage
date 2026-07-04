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
        Assert.Equal("Vui lòng nhập chủ đề yêu cầu.", viewModel.TitleError);
        Assert.Equal("Vui lòng nhập mô tả yêu cầu.", viewModel.DescriptionError);

        viewModel.Title = "Need desk repair";
        viewModel.Description = "The desk hinge is loose.";
        viewModel.CreateTicketCommand.Execute(null);
        await WaitUntilAsync(() => viewModel.HasSuccessMessage);

        Assert.False(viewModel.IsCreateFormOpen);
        Assert.Equal("Đã tạo yêu cầu hỗ trợ.", viewModel.SuccessMessage);
        Assert.Equal("Need desk repair", service.CreatedTickets[0].Title);
    }

    [Fact]
    public async Task Load_limits_recent_ticket_list_to_four_items_and_advances_pages_when_more_tickets_exist()
    {
        var service = new PagedSupportTicketService();
        var viewModel = new SupportTicketListViewModel(service, new StubCurrentUser(RoleNames.Student));

        viewModel.LoadCommand.Execute(null);
        await WaitUntilAsync(() => viewModel.HasTickets);

        Assert.Equal(12, viewModel.TotalTicketCount);
        Assert.Equal(4, viewModel.Tickets.Count);
        Assert.Equal("Hiển thị 1-4 trên 12 yêu cầu", viewModel.TicketFooterSummaryText);
        Assert.False(viewModel.HasPreviousPage);
        Assert.True(viewModel.HasNextPage);

        viewModel.NextPageCommand.Execute(null);

        Assert.Equal("Hiển thị 5-8 trên 12 yêu cầu", viewModel.TicketFooterSummaryText);
        Assert.True(viewModel.HasPreviousPage);
        Assert.True(viewModel.HasNextPage);

        viewModel.NextPageCommand.Execute(null);

        Assert.Equal("Hiển thị 9-12 trên 12 yêu cầu", viewModel.TicketFooterSummaryText);
        Assert.True(viewModel.HasPreviousPage);
        Assert.False(viewModel.HasNextPage);
    }
    [Fact]
    public async Task Create_ticket_command_does_not_force_popup_open_when_popup_is_closed()
    {
        var service = new StubSupportTicketService();
        var viewModel = new SupportTicketListViewModel(service, new StubCurrentUser(RoleNames.Student));

        Assert.False(viewModel.IsCreateFormOpen);

        viewModel.CreateTicketCommand.Execute(null);
        await Task.Delay(50);

        Assert.False(viewModel.IsCreateFormOpen);
        Assert.False(viewModel.HasSuccessMessage);
        Assert.Empty(service.CreatedTickets);
    }

    [Fact]
    public async Task Closing_and_reopening_create_popup_resets_draft_and_validation_state()
    {
        var service = new StubSupportTicketService();
        var viewModel = new SupportTicketListViewModel(service, new StubCurrentUser(RoleNames.Student));

        viewModel.ToggleCreateFormCommand.Execute(null);
        viewModel.Title = "Bóng đèn hỏng";
        viewModel.CreateTicketCommand.Execute(null);
        await Task.Delay(50);

        Assert.True(viewModel.IsCreateFormOpen);
        Assert.True(viewModel.HasTitleError || viewModel.HasDescriptionError);

        viewModel.ToggleCreateFormCommand.Execute(null);
        Assert.False(viewModel.IsCreateFormOpen);

        viewModel.ToggleCreateFormCommand.Execute(null);

        Assert.True(viewModel.IsCreateFormOpen);
        Assert.Equal(string.Empty, viewModel.Title);
        Assert.Equal(string.Empty, viewModel.Description);
        Assert.Null(viewModel.TitleError);
        Assert.Null(viewModel.DescriptionError);
    }


    [Fact]
    public async Task Ticket_screen_button_commands_toggle_select_and_update_expected_state()
    {
        var service = new StubSupportTicketService();
        var viewModel = new SupportTicketListViewModel(service, new StubCurrentUser(RoleNames.Manager));

        viewModel.LoadCommand.Execute(null);
        await WaitUntilAsync(() => viewModel.HasTickets);

        Assert.False(viewModel.IsCreateFormOpen);
        viewModel.ToggleCreateFormCommand.Execute(null);
        Assert.True(viewModel.IsCreateFormOpen);

        Assert.False(viewModel.AreFiltersOpen);
        viewModel.ToggleFiltersCommand.Execute(null);
        Assert.True(viewModel.AreFiltersOpen);

        var selectedTicket = viewModel.Tickets[1];
        viewModel.SelectTicketCommand.Execute(selectedTicket);
        Assert.Same(selectedTicket, viewModel.SelectedTicket);

        viewModel.StatusToApply = SupportTicketStatus.Resolved;
        viewModel.StatusNote = "Done";
        viewModel.UpdateStatusCommand.Execute(null);
        await WaitUntilAsync(() => service.UpdatedStatuses.Count > 0);

        Assert.Equal(selectedTicket.Id, service.UpdatedStatuses[0].TicketId);
        Assert.Equal(SupportTicketStatus.Resolved, service.UpdatedStatuses[0].Status);
        Assert.Equal("Done", service.UpdatedStatuses[0].Note);
        Assert.Equal("Đã cập nhật trạng thái yêu cầu.", viewModel.SuccessMessage);
        Assert.Equal(1, service.GetTicketsAsyncCallCount);
        Assert.NotSame(selectedTicket, viewModel.SelectedTicket);
        Assert.Equal(SupportTicketStatus.Resolved, viewModel.SelectedTicket!.Status);
        Assert.Equal(2, viewModel.OpenTicketCount);
        Assert.Equal(2, viewModel.ResolvedTicketCount);

        var beforeSecondaryAction = viewModel.SelectedTicket;
        viewModel.SecondaryTicketActionCommand.Execute(selectedTicket);
        Assert.Same(beforeSecondaryAction, viewModel.SelectedTicket);
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
        public List<UpdateSupportTicketStatusRequest> UpdatedStatuses { get; } = new();
        public int GetTicketsAsyncCallCount { get; private set; }

        public Task<IReadOnlyList<SupportTicketDto>> GetTicketsAsync(CancellationToken ct = default)
        {
            GetTicketsAsyncCallCount++;
            return Task.FromResult<IReadOnlyList<SupportTicketDto>>(_tickets.ToArray());
        }

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
        public Task UpdateStatusAsync(UpdateSupportTicketStatusRequest request, CancellationToken ct = default)
        {
            UpdatedStatuses.Add(request);
            var ticket = _tickets.FirstOrDefault(candidate => candidate.Id == request.TicketId);
            if (ticket is not null)
            {
                ticket.Status = request.Status;
            }

            return Task.CompletedTask;
        }
        public Task CloseTicketAsync(Guid ticketId, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class PagedSupportTicketService : ISupportTicketService
    {
        private readonly IReadOnlyList<SupportTicketDto> _tickets = Enumerable.Range(1, 12)
            .Select(index => new SupportTicketDto
            {
                Id = Guid.NewGuid(),
                Title = $"Ticket {index}",
                Description = $"Description {index}",
                Category = index % 3 == 0 ? SupportTicketCategory.Maintenance : SupportTicketCategory.Billing,
                Priority = PriorityLevel.Medium,
                Status = index % 4 == 0 ? SupportTicketStatus.Resolved : SupportTicketStatus.New,
                CreatedAt = new DateTime(2026, 6, index)
            })
            .ToArray();

        public Task<IReadOnlyList<SupportTicketDto>> GetTicketsAsync(CancellationToken ct = default) =>
            Task.FromResult(_tickets);

        public Task<SupportTicketDto?> GetTicketAsync(Guid ticketId, CancellationToken ct = default) =>
            Task.FromResult(_tickets.FirstOrDefault(ticket => ticket.Id == ticketId));

        public Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketRequest request, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task AssignTicketAsync(Guid ticketId, Guid managerId, CancellationToken ct = default) => Task.CompletedTask;
        public Task AddResponseAsync(Guid ticketId, string message, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateStatusAsync(UpdateSupportTicketStatusRequest request, CancellationToken ct = default) => Task.CompletedTask;
        public Task CloseTicketAsync(Guid ticketId, CancellationToken ct = default) => Task.CompletedTask;
    }
}





