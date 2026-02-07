using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace QBD.Application.ViewModels;

public abstract partial class RegisterViewModelBase : ViewModelBase
{
    [ObservableProperty]
    private int _accountId;

    [ObservableProperty]
    private string _accountName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<RegisterEntryDto> _entries = new();

    [ObservableProperty]
    private decimal _endingBalance;

    [ObservableProperty]
    private DateTime? _fromDate;

    [ObservableProperty]
    private DateTime? _toDate;

    [ObservableProperty]
    private string _sortColumn = "Date";

    [ObservableProperty]
    private bool _sortDescending;

    [RelayCommand]
    protected abstract Task LoadEntriesAsync();

    [RelayCommand]
    protected virtual void OpenTransaction(RegisterEntryDto entry)
    {
    }
}
