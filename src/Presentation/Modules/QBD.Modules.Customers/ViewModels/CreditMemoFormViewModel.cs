using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Customers;
using QBD.Domain.Entities.Items;
using QBD.Domain.Enums;

namespace QBD.Modules.Customers.ViewModels;

public partial class CreditMemoFormViewModel : TransactionFormViewModelBase<CreditMemo, CreditMemoLine>
{
    private readonly IRepository<CreditMemo> _repository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Item> _itemRepository;
    private readonly INumberSequenceService _numberSequenceService;

    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private ObservableCollection<Item> _items = new();

    public CreditMemoFormViewModel(
        IUnitOfWork unitOfWork, ITransactionPostingService postingService,
        INavigationService navigationService, IRepository<CreditMemo> repository,
        IRepository<Customer> customerRepository, IRepository<Item> itemRepository,
        INumberSequenceService numberSequenceService) : base(unitOfWork, postingService, navigationService)
    {
        _repository = repository; _customerRepository = customerRepository;
        _itemRepository = itemRepository; _numberSequenceService = numberSequenceService;
        Title = "Credit Memo";
    }

    public override async Task InitializeAsync()
    {
        Customers = new ObservableCollection<Customer>(await _customerRepository.Query().Where(c => c.IsActive).OrderBy(c => c.CustomerName).ToListAsync());
        Items = new ObservableCollection<Item>(await _itemRepository.Query().Where(i => i.IsActive).OrderBy(i => i.ItemName).ToListAsync());
        Header = new CreditMemo { CreditNumber = await _numberSequenceService.GetNextNumberAsync("CreditMemo"), Date = DateTime.Today, Status = DocStatus.Draft };
        Lines.Add(new CreditMemoLine());
    }

    protected override void RecalculateTotals()
    {
        SubTotal = Lines.Sum(l => l.Amount);
        GrandTotal = SubTotal;
        Header.Subtotal = SubTotal; Header.Total = GrandTotal; Header.BalanceRemaining = GrandTotal;
    }

    protected override async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            Header.Lines = Lines.Where(l => l.Amount != 0).ToList();
            RecalculateTotals();
            if (Header.Id == 0) await _repository.AddAsync(Header);
            else await _repository.UpdateAsync(Header);
            await UnitOfWork.SaveChangesAsync();
            SetStatus($"Credit Memo {Header.CreditNumber} saved.");
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    protected override async Task SaveAndPostAsync()
    {
        await SaveAsync();
        if (ErrorMessage != null) return;
        try { await PostingService.PostTransactionAsync(TransactionType.CreditMemo, Header.Id); Status = DocStatus.Posted; IsEditable = false; }
        catch (Exception ex) { SetError(ex.Message); }
    }

    protected override async Task VoidAsync()
    {
        if (Header.Id == 0) return;
        try { await PostingService.VoidTransactionAsync(TransactionType.CreditMemo, Header.Id); Status = DocStatus.Voided; IsEditable = false; }
        catch (Exception ex) { SetError(ex.Message); }
    }
}
