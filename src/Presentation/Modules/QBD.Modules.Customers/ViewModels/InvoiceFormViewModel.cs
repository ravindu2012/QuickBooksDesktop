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

public partial class InvoiceFormViewModel : TransactionFormViewModelBase<Invoice, InvoiceLine>
{
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Item> _itemRepository;
    private readonly IRepository<Terms> _termsRepository;
    private readonly INumberSequenceService _numberSequenceService;

    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private ObservableCollection<Item> _items = new();
    [ObservableProperty] private ObservableCollection<Terms> _termsList = new();
    [ObservableProperty] private Customer? _selectedCustomer;
    [ObservableProperty] private Terms? _selectedTerms;

    public InvoiceFormViewModel(
        IUnitOfWork unitOfWork,
        ITransactionPostingService postingService,
        INavigationService navigationService,
        IRepository<Invoice> invoiceRepository,
        IRepository<Customer> customerRepository,
        IRepository<Item> itemRepository,
        IRepository<Terms> termsRepository,
        INumberSequenceService numberSequenceService) : base(unitOfWork, postingService, navigationService)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _itemRepository = itemRepository;
        _termsRepository = termsRepository;
        _numberSequenceService = numberSequenceService;
        Title = "Create Invoice";
    }

    public override async Task InitializeAsync()
    {
        Customers = new ObservableCollection<Customer>(await _customerRepository.Query().Where(c => c.IsActive).OrderBy(c => c.CustomerName).ToListAsync());
        Items = new ObservableCollection<Item>(await _itemRepository.Query().Where(i => i.IsActive).OrderBy(i => i.ItemName).ToListAsync());
        TermsList = new ObservableCollection<Terms>(await _termsRepository.GetAllAsync());

        Header = new Invoice
        {
            InvoiceNumber = await _numberSequenceService.GetNextNumberAsync("Invoice"),
            Date = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            Status = DocStatus.Draft
        };
        Lines.Add(new InvoiceLine());
    }

    partial void OnSelectedCustomerChanged(Customer? value)
    {
        if (value != null)
        {
            Header.CustomerId = value.Id;
            Header.BillToAddress = value.BillToAddress;
            if (value.TermsId.HasValue)
            {
                SelectedTerms = TermsList.FirstOrDefault(t => t.Id == value.TermsId);
            }
        }
    }

    partial void OnSelectedTermsChanged(Terms? value)
    {
        if (value != null)
        {
            Header.TermsId = value.Id;
            Header.DueDate = Header.Date.AddDays(value.DueDays);
        }
    }

    protected override void RecalculateTotals()
    {
        SubTotal = Lines.Sum(l => l.Amount);
        TaxTotal = 0; // Simplified - would calculate from tax codes
        GrandTotal = SubTotal + TaxTotal;
        BalanceDue = GrandTotal;
        Header.Subtotal = SubTotal;
        Header.TaxTotal = TaxTotal;
        Header.Total = GrandTotal;
        Header.BalanceDue = BalanceDue;
    }

    protected override async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            Header.Lines = Lines.Where(l => l.Amount != 0).ToList();
            RecalculateTotals();

            if (Header.Id == 0)
                await _invoiceRepository.AddAsync(Header);
            else
                await _invoiceRepository.UpdateAsync(Header);

            await UnitOfWork.SaveChangesAsync();
            SetStatus($"Invoice {Header.InvoiceNumber} saved.");
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
            await PostingService.PostTransactionAsync(TransactionType.Invoice, Header.Id);
            Status = DocStatus.Posted;
            IsEditable = false;
            SetStatus($"Invoice {Header.InvoiceNumber} posted to GL.");
        }
        catch (Exception ex) { SetError(ex.Message); }
    }

    protected override async Task VoidAsync()
    {
        if (Header.Id == 0) return;
        try
        {
            await PostingService.VoidTransactionAsync(TransactionType.Invoice, Header.Id);
            Header.Status = DocStatus.Voided;
            Status = DocStatus.Voided;
            IsEditable = false;
            SetStatus($"Invoice {Header.InvoiceNumber} voided.");
        }
        catch (Exception ex) { SetError(ex.Message); }
    }
}
