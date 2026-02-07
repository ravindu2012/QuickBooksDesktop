using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace QBD.Application.ViewModels;

public abstract partial class ListViewModelBase<TDto> : ViewModelBase where TDto : class
{
    [ObservableProperty]
    private ObservableCollection<TDto> _items = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 50;

    [ObservableProperty]
    private int _totalRecords;

    [ObservableProperty]
    private TDto? _selectedItem;

    [RelayCommand]
    protected abstract Task LoadItemsAsync();

    [RelayCommand]
    protected virtual void NewItem() { }

    [RelayCommand]
    protected virtual void EditItem(TDto item) { }

    [RelayCommand]
    protected virtual void DeleteItem(TDto item) { }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadItemsAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage * PageSize < TotalRecords)
        {
            CurrentPage++;
            await LoadItemsAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadItemsAsync();
        }
    }
}
