using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.Application.DTOs.Students;
using DormitoryManagement.Application.Services.Students;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Students;

public sealed class StudentListViewModel : ViewModelBase
{
    private readonly IStudentService _studentService;
    private bool _hasLoaded;
    private int _totalCount;

    public StudentListViewModel(IStudentService studentService)
    {
        _studentService = studentService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    public ObservableCollection<StudentDto> Students { get; } = new();
    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand => LoadCommand;
    public bool HasStudents => Students.Count > 0;
    public bool IsStudentsEmpty => _hasLoaded && !IsBusy && Students.Count == 0;

    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    private async Task LoadAsync()
    {
        ClearError();
        IsBusy = true;
        NotifyUiState();
        try
        {
            var result = await _studentService.GetStudentsAsync(1, 200);
            Students.Clear();
            foreach (var student in result.Items)
            {
                Students.Add(student);
            }

            TotalCount = result.TotalCount;
            _hasLoaded = true;
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyUiState();
        }
    }

    private void NotifyUiState()
    {
        OnPropertyChanged(nameof(HasStudents));
        OnPropertyChanged(nameof(IsStudentsEmpty));
    }
}
