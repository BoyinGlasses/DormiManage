namespace DormitoryManagement.WPF.Common;

public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string? _errorMessage;

    public bool IsBusy
    {
        get => _isBusy;
        protected set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }

    public bool IsLoading => IsBusy;

    public string? ErrorMessage
    {
        get => _errorMessage;
        protected set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    protected void ClearError() => ErrorMessage = null;
    protected void SetError(string? message) => ErrorMessage = message;
}
