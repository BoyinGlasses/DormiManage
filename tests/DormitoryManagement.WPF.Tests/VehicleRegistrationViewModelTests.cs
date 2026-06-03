using DormitoryManagement.Application.DTOs.Vehicles;
using DormitoryManagement.Application.Services.Vehicles;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.ViewModels.Vehicles;

namespace DormitoryManagement.WPF.Tests;

public sealed class VehicleRegistrationViewModelTests
{
    [Fact]
    public void Constructor_exposes_reference_review_duration_options_default_subtotal_and_history_columns()
    {
        var viewModel = new VehicleRegistrationViewModel(new StubVehicleService());

        Assert.Equal(new[] { 1, 2, 3, 6 }, viewModel.MonthOptions);
        Assert.Equal(1, viewModel.SelectedMonthCount);
        Assert.Equal("40.000 ₫", viewModel.SubtotalAmountText);
        Assert.Equal(new[]
        {
            "Ngày đăng ký",
            "Biển số xe",
            "Thời hạn",
            "Tổng tiền",
            "Trạng thái"
        }, viewModel.HistoryColumnHeaders);
        Assert.False(viewModel.HasVehicles);
    }

    [Fact]
    public void Selected_month_count_updates_review_subtotal_text()
    {
        var viewModel = new VehicleRegistrationViewModel(new StubVehicleService());

        viewModel.SelectedMonthCount = 3;

        Assert.Equal(120000m, viewModel.PreviewAmount);
        Assert.Equal("120.000 ₫", viewModel.SubtotalAmountText);
    }

    [Fact]
    public async Task LoadCommand_preserves_long_plate_text_in_history_review_rows()
    {
        const string longPlate = "59A1-12345-SINHVIEN-KTX-TEST";
        var viewModel = new VehicleRegistrationViewModel(new StubVehicleService(
        [
            new VehicleRegistrationDto
            {
                Id = Guid.NewGuid(),
                LicensePlate = longPlate,
                NormalizedPlate = longPlate,
                MonthCount = 6,
                Amount = 240000m,
                Status = VehicleStatus.Approved,
                StatusText = "Đã duyệt",
                RegisteredAt = new DateTime(2026, 6, 1)
            }
        ]));

        viewModel.LoadCommand.Execute(null);

        Assert.True(await WaitUntilAsync(() => viewModel.HistoryReviewRows.Count == 1));
        var row = Assert.Single(viewModel.HistoryReviewRows);
        Assert.Equal(longPlate, row.LicensePlateText);
        Assert.Equal("Đã duyệt", row.StatusLabel);
        Assert.Equal("240.000 ₫", row.TotalAmountText);
    }

    [Fact]
    public async Task SubmitCommand_sends_license_plate_and_selected_month_to_service()
    {
        var service = new StubVehicleService();
        var viewModel = new VehicleRegistrationViewModel(service)
        {
            LicensePlate = "59a12345",
            SelectedMonthCount = 6
        };

        viewModel.SubmitCommand.Execute(null);

        var request = await service.WaitForRequestAsync();
        Assert.Equal("59a12345", request.LicensePlate);
        Assert.Equal(6, request.MonthCount);
        Assert.True(await WaitUntilAsync(() => viewModel.Vehicles.Count == 1));
        Assert.Equal("Đăng ký giữ xe thành công. Hóa đơn đã được tạo trong Billing.", viewModel.SuccessMessage);
    }

    private static async Task<bool> WaitUntilAsync(Func<bool> condition)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        while (!cts.IsCancellationRequested)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(10, cts.Token);
        }

        return false;
    }

    private sealed class StubVehicleService : IVehicleService
    {
        private readonly TaskCompletionSource<CreateVehicleRegistrationRequest> _requestSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly IReadOnlyList<VehicleRegistrationDto> _registrations;

        public StubVehicleService(IReadOnlyList<VehicleRegistrationDto>? registrations = null)
        {
            _registrations = registrations ?? Array.Empty<VehicleRegistrationDto>();
        }

        public Task<CreateVehicleRegistrationRequest> WaitForRequestAsync() => _requestSource.Task;

        public Task<IReadOnlyList<VehicleRegistrationDto>> GetCurrentStudentVehicleRegistrationsAsync(DateTime? asOfDate = null, CancellationToken ct = default) =>
            Task.FromResult(_registrations);

        public Task<VehicleRegistrationDto> RegisterVehicleAsync(CreateVehicleRegistrationRequest request, CancellationToken ct = default)
        {
            _requestSource.TrySetResult(request);
            return Task.FromResult(new VehicleRegistrationDto
            {
                Id = Guid.NewGuid(),
                LicensePlate = "59A1-2345",
                NormalizedPlate = "59A1-2345",
                MonthCount = request.MonthCount,
                Amount = request.MonthCount * 40000m,
                Status = VehicleStatus.Pending,
                StatusText = "Chưa thanh toán"
            });
        }

        public Task ApproveVehicleAsync(Guid registrationId, CancellationToken ct = default) => Task.CompletedTask;
        public Task RejectVehicleAsync(Guid registrationId, string reason, CancellationToken ct = default) => Task.CompletedTask;
        public Task CancelVehicleAsync(Guid registrationId, CancellationToken ct = default) => Task.CompletedTask;
    }
}

