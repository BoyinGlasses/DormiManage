using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Students;

namespace DormitoryManagement.Application.Services.Students;

public interface IStudentService
{
    Task<PagedResult<StudentDto>> GetStudentsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
    Task<StudentDto?> GetStudentByIdAsync(Guid id, CancellationToken ct = default);
    Task<StudentProfileDto> GetCurrentStudentProfileAsync(CancellationToken ct = default);
    Task<StudentDto> CreateStudentAsync(CreateStudentRequest request, CancellationToken ct = default);
    Task<StudentDto> UpdateStudentAsync(UpdateStudentRequest request, CancellationToken ct = default);
    Task DeactivateStudentAsync(Guid id, CancellationToken ct = default);
}
