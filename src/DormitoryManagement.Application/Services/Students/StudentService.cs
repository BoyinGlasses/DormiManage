using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Students;
using DormitoryManagement.Application.Validation;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;

namespace DormitoryManagement.Application.Services.Students;

public sealed class StudentService : IStudentService
{
    private const string DefaultProfileAvatarAssetPath = "pack://application:,,,/DormitoryManagement.WPF;component/Assets/Images/Profile/profile-avatar.png";

    private readonly IStudentRepository _students;
    private readonly IPermissionService _permissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public StudentService(
        IStudentRepository students,
        IPermissionService permissions,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _students = students;
        _permissions = permissions;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<StudentDto>> GetStudentsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.StudentsRead, ct);
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);
        var students = _students.Query()
            .Where(student => !student.IsDeleted)
            .OrderBy(student => student.StudentCode)
            .ToList();

        students = ApplyCurrentUserScope(students);
        var totalCount = students.Count;
        var page = students
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<StudentDto>(MapStudents(page), totalCount, pageNumber, pageSize);
    }

    public async Task<StudentDto?> GetStudentByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.StudentsRead, ct);
        var student = await _students.GetByIdAsync(id, ct);
        if (student is null || student.IsDeleted)
        {
            return null;
        }

        EnsureCanReadStudent(student);
        return MapStudents(new[] { student }).Single();
    }

    public async Task<StudentProfileDto> GetCurrentStudentProfileAsync(CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.StudentsRead, ct);

        var student = ResolveCurrentStudent();
        EnsureCanReadStudent(student);

        var room = ResolveCurrentRoom(student);
        var building = room is null ? null : _unitOfWork.Repository<Building>().Query().FirstOrDefault(candidate => candidate.Id == room.BuildingId);
        var hasActiveAssignment = room is not null;
        var buildingDisplayName = ResolveBuildingDisplayName(building);
        var roomDisplayLabel = ResolveRoomDisplayLabel(building, room);

        return new StudentProfileDto
        {
            StudentId = student.Id,
            FullName = student.FullName,
            StudentCode = student.StudentCode,
            Email = student.Email ?? string.Empty,
            PhoneNumber = student.PhoneNumber ?? string.Empty,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender ?? string.Empty,
            AvatarAssetPath = DefaultProfileAvatarAssetPath,
            ProfileStatusMessage = ResolveProfileStatusMessage(student),
            BuildingName = buildingDisplayName,
            RoomLabel = roomDisplayLabel,
            AssignmentStatus = hasActiveAssignment ? "Đang ở" : "Chưa phân phòng",
            DormitorySupportMessage = hasActiveAssignment
                ? "Thông tin phòng hiện tại của bạn được đồng bộ từ hồ sơ nội trú."
                : "Bạn chưa có phòng nội trú đang hoạt động.",
            HasActiveAssignment = hasActiveAssignment
        };
    }

    public async Task<StudentDto> CreateStudentAsync(CreateStudentRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.StudentsWrite, ct);
        RequestValidator.ValidateAndThrow(request);
        return new StudentDto { Id = Guid.NewGuid(), StudentCode = request.StudentCode, FullName = request.FullName, Email = request.Email };
    }

    public async Task<StudentDto> UpdateStudentAsync(UpdateStudentRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.StudentsWrite, ct);
        RequestValidator.ValidateAndThrow(request);
        return new StudentDto { Id = request.StudentId, FullName = request.FullName, Email = request.Email };
    }

    public async Task DeactivateStudentAsync(Guid id, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.StudentsWrite, ct);
    }

    private List<Student> ApplyCurrentUserScope(List<Student> students)
    {
        if (_currentUser.IsInRole(RoleNames.Student))
        {
            var studentId = ResolveCurrentStudentId();
            return students.Where(student => student.Id == studentId).ToList();
        }

        if (_currentUser.CurrentUser?.BuildingId is { } buildingId)
        {
            var roomIds = _unitOfWork.Repository<Room>().Query()
                .Where(room => room.BuildingId == buildingId)
                .Select(room => room.Id)
                .ToHashSet();
            var activeStudentIds = _unitOfWork.Repository<RoomAssignment>().Query()
                .Where(assignment => assignment.IsActive && roomIds.Contains(assignment.RoomId))
                .Select(assignment => assignment.StudentId)
                .ToHashSet();

            return students.Where(student =>
                    activeStudentIds.Contains(student.Id)
                    || (student.CurrentRoomId.HasValue && roomIds.Contains(student.CurrentRoomId.Value)))
                .ToList();
        }

        return students;
    }

    private void EnsureCanReadStudent(Student student)
    {
        if (_currentUser.IsInRole(RoleNames.Student) && student.Id != ResolveCurrentStudentId())
        {
            throw new InvalidOperationException("Students can view only their own profile.");
        }

        if (_currentUser.CurrentUser?.BuildingId is { } buildingId && !IsStudentInBuilding(student, buildingId))
        {
            throw new InvalidOperationException("Building managers can view only students in their assigned building.");
        }
    }

    private bool IsStudentInBuilding(Student student, Guid buildingId)
    {
        var roomIds = _unitOfWork.Repository<Room>().Query()
            .Where(room => room.BuildingId == buildingId)
            .Select(room => room.Id)
            .ToHashSet();

        if (student.CurrentRoomId.HasValue && roomIds.Contains(student.CurrentRoomId.Value))
        {
            return true;
        }

        return _unitOfWork.Repository<RoomAssignment>().Query()
            .Any(assignment => assignment.StudentId == student.Id && assignment.IsActive && roomIds.Contains(assignment.RoomId));
    }

    private Student ResolveCurrentStudent()
    {
        var studentId = ResolveCurrentStudentId();
        return _students.Query().FirstOrDefault(candidate => candidate.Id == studentId && !candidate.IsDeleted)
            ?? throw new InvalidOperationException("Current user is not linked to a student profile.");
    }

    private Guid ResolveCurrentStudentId()
    {
        if (_currentUser.CurrentUser?.StudentId is { } currentStudentId)
        {
            return currentStudentId;
        }

        if (_currentUser.UserId is { } userId)
        {
            var student = _students.Query().FirstOrDefault(candidate => candidate.UserId == userId && !candidate.IsDeleted);
            if (student is not null)
            {
                return student.Id;
            }
        }

        throw new InvalidOperationException("Current user is not linked to a student profile.");
    }

    private Room? ResolveCurrentRoom(Student student)
    {
        var roomId = _unitOfWork.Repository<RoomAssignment>().Query()
            .Where(assignment => assignment.StudentId == student.Id && assignment.IsActive)
            .OrderByDescending(assignment => assignment.StartDate)
            .Select(assignment => (Guid?)assignment.RoomId)
            .FirstOrDefault()
            ?? student.CurrentRoomId;

        if (!roomId.HasValue)
        {
            return null;
        }

        return _unitOfWork.Repository<Room>().Query().FirstOrDefault(candidate => candidate.Id == roomId.Value);
    }

    private static string ResolveBuildingDisplayName(Building? building)
    {
        if (building is null)
        {
            return "Chưa cập nhật";
        }

        return string.IsNullOrWhiteSpace(building.Name)
            ? string.IsNullOrWhiteSpace(building.Code) ? "Chưa cập nhật" : building.Code
            : building.Name;
    }

    private static string ResolveRoomDisplayLabel(Building? building, Room? room)
    {
        if (room is null)
        {
            return "Chưa cập nhật";
        }

        if (!string.IsNullOrWhiteSpace(building?.Code))
        {
            return $"{building.Code}-{room.RoomNumber}";
        }

        return room.RoomNumber;
    }

    private static string ResolveProfileStatusMessage(Student student)
    {
        return student.Status switch
        {
            Domain.Enums.StudentStatus.Staying => "Sinh viên nội trú đang hoạt động",
            Domain.Enums.StudentStatus.Pending => "Hồ sơ sinh viên đang chờ xử lý",
            Domain.Enums.StudentStatus.Left => "Sinh viên đã rời ký túc xá",
            Domain.Enums.StudentStatus.NotRegistered => "Chưa hoàn tất đăng ký nội trú",
            _ => "Thông tin hồ sơ sinh viên"
        };
    }

    private IReadOnlyList<StudentDto> MapStudents(IReadOnlyList<Student> students)
    {
        if (students.Count == 0)
        {
            return Array.Empty<StudentDto>();
        }

        var studentIds = students.Select(student => student.Id).ToHashSet();
        var activeRoomIds = _unitOfWork.Repository<RoomAssignment>().Query()
            .Where(assignment => assignment.IsActive && studentIds.Contains(assignment.StudentId))
            .GroupBy(assignment => assignment.StudentId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(assignment => assignment.StartDate).First().RoomId);

        return students.Select(student => new StudentDto
        {
            Id = student.Id,
            StudentCode = student.StudentCode,
            FullName = student.FullName,
            Email = student.Email,
            PhoneNumber = student.PhoneNumber,
            Status = student.Status,
            CurrentRoomId = student.CurrentRoomId ?? (activeRoomIds.TryGetValue(student.Id, out var roomId) ? roomId : null)
        }).ToArray();
    }
}
