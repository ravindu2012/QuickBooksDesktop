using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Customers;
using QBD.Domain.Enums;

namespace QBD.Modules.Customers.ViewModels;

public partial class ReceivePaymentFormViewModel : TransactionFormViewModelBase<ReceivePayment, PaymentApplication>
{
    private readonly IRepository<ReceivePayment> _paymentRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<PaymentMethod> _paymentMethodRepository;

    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private ObservableCollection<Invoice> _openInvoices = new();
    [ObservableProperty] private ObservableCollection<Account> _depositAccounts = new();
    [ObservableProperty] private ObservableCollection<PaymentMethod> _paymentMethods = new();
    [ObservableProperty] private Customer? _selectedCustomer;

    public ReceivePaymentFormViewModel(
        IUnitOfWork unitOfWork,
        ITransactionPostingService postingService,
        INavigationService navigationService,
        IRepository<ReceivePayment> paymentRepository,
        IRepository<Customer> customerRepository,
        IRepository<Invoice> invoiceRepository,
        IRepository<Account> accountRepository,
        IRepository<PaymentMethod> paymentMethodRepository) : base(unitOfWork, postingService, navigationService)
    {
        _paymentRepository = paymentRepository;
        _customerRepository = customerRepository;
        _invoiceRepository = invoiceRepository;
        _accountRepository = accountRepository;
        _paymentMethodRepository = paymentMethodRepository;
        Title = "Receive Payments";
    }

    public override async Task InitializeAsync()
    {
        Customers = new ObservableCollection<Customer>(await _customerRepository.Query().Where(c => c.IsActive).OrderBy(c => c.CustomerName).ToListAsync());
        DepositAccounts = new ObservableCollection<Account>(await _accountRepository.Query().Where(a => a.AccountType == AccountType.Bank || a.Name == "Undeposited Funds").ToListAsync());
        PaymentMethods = new ObservableCollection<PaymentMethod>(await _paymentMethodRepository.GetAllAsync());
        Header = new ReceivePayment { PaymentDate = DateTime.Today, Status = DocStatus.Draft };
    }

    partial void OnSelectedCustomerChanged(Customer? value)
    {
        if (value != null)
        {
            Header.CustomerId = value.Id;
            _ = LoadOpenInvoicesAsync(value.Id);
        }
    }

    private async Task LoadOpenInvoicesAsync(int customerId)
    {
        var invoices = await _invoiceRepository.FindAsync(i => i.CustomerId == customerId && i.BalanceDue > 0 && i.Status == DocStatus.Posted);
        OpenInvoices = new ObservableCollection<Invoice>(invoices);
        Lines.Clear();
        foreach (var inv in invoices)
        {
            Lines.Add(new PaymentApplication { InvoiceId = inv.Id, AmountApplied = inv.BalanceDue });
        }
        RecalculateTotals();
    }

    protected override void RecalculateTotals()
    {
        GrandTotal = Lines.Sum(l => l.AmountApplied);
        Header.Amount = GrandTotal;
    }

    protected override async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            Header.Applications = Lines.Where(l => l.AmountApplied > 0).ToList();
            RecalculateTotals();
            await _paymentRepository.AddAsync(Header);
            await UnitOfWork.SaveChangesAsync();
            SetStatus("Payment saved.");
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
            await PostingService.PostTransactionAsync(TransactionType.ReceivePayment, Header.Id);
            Status = DocStatus.Posted;
            IsEditable = false;
            SetStatus("Payment posted to GL.");
        }
        catch (Exception ex) { SetError(ex.Message); }
    }

    protected override async Task VoidAsync()
    {
        if (Header.Id == 0) return;
        try
        {
            await PostingService.VoidTransactionAsync(TransactionType.ReceivePayment, Header.Id);
            Status = DocStatus.Voided;
            IsEditable = false;
            SetStatus("Payment voided.");
        }
        catch (Exception ex) { SetError(ex.Message); }
    }
}
