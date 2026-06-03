using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Students;
using DormitoryManagement.Application.Services.Students;
using DormitoryManagement.WPF.ViewModels.Students;

namespace DormitoryManagement.WPF.Tests;

public sealed class StudentProfileViewModelTests
{
    [Fact]
    public async Task LoadCommand_populates_profile_fields_and_dormitory_summary()
    {
        var service = new StubStudentService(new StudentProfileDto
        {
            StudentId = Guid.NewGuid(),
            FullName = "Nguyễn Văn A",
            StudentCode = "24521111",
            Email = "nguyen.van.a@student.edu.vn",
            PhoneNumber = "+84 123 456 789",
            DateOfBirth = new DateTime(2002, 8, 15),
            Gender = "Nam",
            ProfileStatusMessage = "Sinh viên nội trú đang hoạt động",
            BuildingName = "Tòa nhà A",
            RoomLabel = "A-101",
            AssignmentStatus = "Đang ở",
            DormitorySupportMessage = "Thông tin phòng hiện tại của bạn được đồng bộ từ hồ sơ nội trú.",
            HasActiveAssignment = true
        });
        var viewModel = new StudentProfileViewModel(service);

        viewModel.LoadCommand.Execute(null);
        await WaitUntilAsync(() => viewModel.HasLoaded);

        Assert.Equal("Nguyễn Văn A", viewModel.FullName);
        Assert.Equal("24521111", viewModel.StudentCode);
        Assert.Equal("MSSV: 24521111", viewModel.StudentCodeBadgeText);
        Assert.Equal("Tòa nhà A", viewModel.BuildingName);
        Assert.Equal("A-101", viewModel.RoomLabel);
        Assert.True(viewModel.HasActiveAssignment);
        Assert.Equal("Đang ở", viewModel.AssignmentStatus);
    }

    [Fact]
    public async Task LoadCommand_uses_fallback_values_when_optional_profile_fields_are_missing()
    {
        var service = new StubStudentService(new StudentProfileDto
        {
            StudentId = Guid.NewGuid(),
            FullName = "Nguyễn Văn A",
            StudentCode = "24521111",
            AssignmentStatus = "Chưa phân phòng",
            DormitorySupportMessage = "Bạn chưa có phòng nội trú đang hoạt động.",
            HasActiveAssignment = false
        });
        var viewModel = new StudentProfileViewModel(service);

        viewModel.LoadCommand.Execute(null);
        await WaitUntilAsync(() => viewModel.HasLoaded);

        Assert.Equal("Chưa cập nhật", viewModel.Email);
        Assert.Equal("Chưa cập nhật", viewModel.PhoneNumber);
        Assert.Equal("Chưa cập nhật", viewModel.DateOfBirth);
        Assert.Equal("Chưa cập nhật", viewModel.Gender);
        Assert.Equal("Chưa cập nhật", viewModel.BuildingName);
        Assert.Equal("Chưa cập nhật", viewModel.RoomLabel);
        Assert.False(viewModel.HasActiveAssignment);
    }

    [Fact]
    public async Task LoadCommand_surfaces_error_when_profile_cannot_be_resolved()
    {
        var viewModel = new StudentProfileViewModel(new ThrowingStudentService());

        viewModel.LoadCommand.Execute(null);
        await WaitUntilAsync(() => viewModel.HasError);

        Assert.Equal("Current user is not linked to a student profile.", viewModel.ErrorMessage);
    }

    [Fact]
    public void Edit_and_setting_commands_remain_click_safe_and_non_destructive()
    {
        var viewModel = new StudentProfileViewModel(new StubStudentService(new StudentProfileDto { FullName = "Nguyễn Văn A", StudentCode = "24521111" }));
        var setting = viewModel.AccountSettings[0];

        viewModel.EditProfileCommand.Execute(null);
        Assert.True(viewModel.HasActionMessage);
        Assert.Contains("Chỉnh sửa hồ sơ", viewModel.ActionMessage);

        viewModel.OpenSettingCommand.Execute(setting);
        Assert.Contains(setting.Label, viewModel.ActionMessage);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        while (!cts.IsCancellationRequested)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(10, cts.Token);
        }

        throw new TimeoutException("Condition was not met in time.");
    }

    private sealed class StubStudentService : IStudentService
    {
        private readonly StudentProfileDto _profile;

        public StubStudentService(StudentProfileDto profile)
        {
            _profile = profile;
        }

        public Task<PagedResult<StudentDto>> GetStudentsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<StudentDto?> GetStudentByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<StudentProfileDto> GetCurrentStudentProfileAsync(CancellationToken ct = default) => Task.FromResult(_profile);
        public Task<StudentDto> CreateStudentAsync(CreateStudentRequest request, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<StudentDto> UpdateStudentAsync(UpdateStudentRequest request, CancellationToken ct = default) => throw new NotSupportedException();
        public Task DeactivateStudentAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class ThrowingStudentService : IStudentService
    {
        public Task<PagedResult<StudentDto>> GetStudentsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<StudentDto?> GetStudentByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<StudentProfileDto> GetCurrentStudentProfileAsync(CancellationToken ct = default) => throw new InvalidOperationException("Current user is not linked to a student profile.");
        public Task<StudentDto> CreateStudentAsync(CreateStudentRequest request, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<StudentDto> UpdateStudentAsync(UpdateStudentRequest request, CancellationToken ct = default) => throw new NotSupportedException();
        public Task DeactivateStudentAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    }
}
