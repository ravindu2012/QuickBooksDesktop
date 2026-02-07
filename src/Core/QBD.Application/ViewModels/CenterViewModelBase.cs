using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QBD.Application.Interfaces;

namespace QBD.Application.ViewModels;

public abstract partial class CenterViewModelBase<TEntity, TDetail> : ViewModelBase
    where TEntity : class
    where TDetail : class
{
    protected readonly INavigationService NavigationService;

    [ObservableProperty]
    private ObservableCollection<TEntity> _entityList = new();

    [ObservableProperty]
    private TEntity? _selectedEntity;

    [ObservableProperty]
    private TDetail? _entityDetail;

    [ObservableProperty]
    private ObservableCollection<TransactionSummaryDto> _transactions = new();

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private string _activeFilter = "All";

    protected CenterViewModelBase(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    partial void OnSelectedEntityChanged(TEntity? value)
    {
        if (value != null)
        {
            _ = LoadEntityDetailAsync(value);
            _ = LoadTransactionsAsync(value);
        }
    }

    protected abstract Task LoadEntitiesAsync();
    protected abstract Task LoadEntityDetailAsync(TEntity entity);
    protected abstract Task LoadTransactionsAsync(TEntity entity);

    [RelayCommand]
    protected virtual void NewEntity()
    {
    }

    [RelayCommand]
    protected virtual void EditEntity()
    {
    }

    [RelayCommand]
    protected virtual void OpenTransaction(TransactionSummaryDto transaction)
    {
    }

    [RelayCommand]
    private async Task FilterAsync()
    {
        await LoadEntitiesAsync();
    }
}
