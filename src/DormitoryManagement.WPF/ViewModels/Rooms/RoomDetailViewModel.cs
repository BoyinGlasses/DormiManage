using DormitoryManagement.Application.Services.Rooms;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Rooms;

public sealed class RoomDetailViewModel : ViewModelBase
{
    private readonly IRoomService _roomService;

    public RoomDetailViewModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public IRoomService RoomService => _roomService;
}
