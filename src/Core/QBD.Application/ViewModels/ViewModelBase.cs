using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace QBD.Application.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _statusMessage;

    public virtual Task InitializeAsync() => Task.CompletedTask;

    [RelayCommand]
    protected virtual Task RefreshAsync() => Task.CompletedTask;

    protected void ClearError() => ErrorMessage = null;

    protected void SetError(string message) => ErrorMessage = message;

    protected void SetStatus(string message) => StatusMessage = message;
}
