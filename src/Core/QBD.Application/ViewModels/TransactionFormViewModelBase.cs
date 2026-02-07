using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QBD.Application.Interfaces;
using QBD.Domain.Enums;

namespace QBD.Application.ViewModels;

public abstract partial class TransactionFormViewModelBase<THeader, TLine> : ViewModelBase
    where THeader : class, new()
    where TLine : class, new()
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly ITransactionPostingService PostingService;
    protected readonly INavigationService NavigationService;

    [ObservableProperty]
    private THeader _header = new();

    [ObservableProperty]
    private ObservableCollection<TLine> _lines = new();

    [ObservableProperty]
    private decimal _subTotal;

    [ObservableProperty]
    private decimal _taxTotal;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private decimal _balanceDue;

    [ObservableProperty]
    private DocStatus _status = DocStatus.Draft;

    [ObservableProperty]
    private bool _isEditable = true;

    protected TransactionFormViewModelBase(
        IUnitOfWork unitOfWork,
        ITransactionPostingService postingService,
        INavigationService navigationService)
    {
        UnitOfWork = unitOfWork;
        PostingService = postingService;
        NavigationService = navigationService;
    }

    [RelayCommand]
    protected virtual void AddLine()
    {
        Lines.Add(new TLine());
    }

    [RelayCommand]
    protected virtual void RemoveLine(TLine line)
    {
        Lines.Remove(line);
        RecalculateTotals();
    }

    [RelayCommand]
    protected abstract Task SaveAsync();

    [RelayCommand]
    protected abstract Task SaveAndPostAsync();

    [RelayCommand]
    protected abstract Task VoidAsync();

    [RelayCommand]
    protected virtual void Clear()
    {
        Header = new THeader();
        Lines.Clear();
        SubTotal = 0;
        TaxTotal = 0;
        GrandTotal = 0;
        BalanceDue = 0;
        Status = DocStatus.Draft;
        IsEditable = true;
    }

    [RelayCommand]
    protected virtual void Print()
    {
        // Print functionality
    }

    protected abstract void RecalculateTotals();
}
