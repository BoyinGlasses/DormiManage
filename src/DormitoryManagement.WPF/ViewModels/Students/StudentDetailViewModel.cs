using DormitoryManagement.Application.DTOs.Students;
using DormitoryManagement.Application.Services.Students;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Students;

public sealed class StudentDetailViewModel : ViewModelBase
{
    private readonly IStudentService _studentService;
    private StudentDto? _student;

    public StudentDetailViewModel(IStudentService studentService)
    {
        _studentService = studentService;
    }

    public StudentDto? Student
    {
        get => _student;
        private set => SetProperty(ref _student, value);
    }

    public async Task LoadAsync(Guid studentId, CancellationToken ct = default) => Student = await _studentService.GetStudentByIdAsync(studentId, ct);
}
