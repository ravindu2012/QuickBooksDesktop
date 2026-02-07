using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Customers;
using QBD.Domain.Entities.Items;
using QBD.Domain.Enums;

namespace QBD.Modules.Customers.ViewModels;

public partial class SalesReceiptFormViewModel : TransactionFormViewModelBase<SalesReceipt, SalesReceiptLine>
{
    private readonly IRepository<SalesReceipt> _repository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Item> _itemRepository;
    private readonly INumberSequenceService _numberSequenceService;

    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private ObservableCollection<Item> _items = new();

    public SalesReceiptFormViewModel(
        IUnitOfWork unitOfWork, ITransactionPostingService postingService,
        INavigationService navigationService, IRepository<SalesReceipt> repository,
        IRepository<Customer> customerRepository, IRepository<Item> itemRepository,
        INumberSequenceService numberSequenceService) : base(unitOfWork, postingService, navigationService)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _itemRepository = itemRepository;
        _numberSequenceService = numberSequenceService;
        Title = "Sales Receipt";
    }

    public override async Task InitializeAsync()
    {
        Customers = new ObservableCollection<Customer>(await _customerRepository.Query().Where(c => c.IsActive).OrderBy(c => c.CustomerName).ToListAsync());
        Items = new ObservableCollection<Item>(await _itemRepository.Query().Where(i => i.IsActive).OrderBy(i => i.ItemName).ToListAsync());
        Header = new SalesReceipt
        {
            SalesReceiptNumber = await _numberSequenceService.GetNextNumberAsync("SalesReceipt"),
            Date = DateTime.Today, Status = DocStatus.Draft
        };
        Lines.Add(new SalesReceiptLine());
    }

    protected override void RecalculateTotals()
    {
        SubTotal = Lines.Sum(l => l.Amount);
        GrandTotal = SubTotal;
        Header.Subtotal = SubTotal;
        Header.Total = GrandTotal;
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
            SetStatus($"Sales Receipt {Header.SalesReceiptNumber} saved.");
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    protected override async Task SaveAndPostAsync()
    {
        await SaveAsync();
        if (ErrorMessage != null) return;
        try
        {
            await PostingService.PostTransactionAsync(TransactionType.SalesReceipt, Header.Id);
            Status = DocStatus.Posted; IsEditable = false;
            SetStatus($"Sales Receipt posted to GL.");
        }
        catch (Exception ex) { SetError(ex.Message); }
    }

    protected override async Task VoidAsync()
    {
        if (Header.Id == 0) return;
        try { await PostingService.VoidTransactionAsync(TransactionType.SalesReceipt, Header.Id); Status = DocStatus.Voided; IsEditable = false; }
        catch (Exception ex) { SetError(ex.Message); }
    }
}
