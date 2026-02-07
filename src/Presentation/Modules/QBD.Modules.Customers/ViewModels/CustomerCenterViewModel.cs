using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Customers;
using QBD.Domain.Enums;

namespace QBD.Modules.Customers.ViewModels;

public partial class CustomerCenterViewModel : CenterViewModelBase<Customer, Customer>
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<ReceivePayment> _paymentRepository;

    public CustomerCenterViewModel(
        INavigationService navigationService,
        IRepository<Customer> customerRepository,
        IRepository<Invoice> invoiceRepository,
        IRepository<ReceivePayment> paymentRepository) : base(navigationService)
    {
        _customerRepository = customerRepository;
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        Title = "Customer Center";
    }

    public override async Task InitializeAsync() => await LoadEntitiesAsync();

    protected override async Task LoadEntitiesAsync()
    {
        var query = _customerRepository.Query();
        if (!string.IsNullOrWhiteSpace(FilterText))
            query = query.Where(c => c.CustomerName.Contains(FilterText));
        if (ActiveFilter == "Active")
            query = query.Where(c => c.IsActive);

        var customers = await query.OrderBy(c => c.CustomerName).ToListAsync();
        EntityList = new ObservableCollection<Customer>(customers);
    }

    protected override Task LoadEntityDetailAsync(Customer entity)
    {
        EntityDetail = entity;
        return Task.CompletedTask;
    }

    protected override async Task LoadTransactionsAsync(Customer entity)
    {
        var transactions = new List<TransactionSummaryDto>();

        var invoices = await _invoiceRepository.FindAsync(i => i.CustomerId == entity.Id);
        foreach (var inv in invoices)
        {
            transactions.Add(new TransactionSummaryDto
            {
                Id = inv.Id, Type = TransactionType.Invoice, TypeDisplay = "Invoice",
                Number = inv.InvoiceNumber, Date = inv.Date, Amount = inv.Total,
                Balance = inv.BalanceDue, Status = inv.Status
            });
        }

        var payments = await _paymentRepository.FindAsync(p => p.CustomerId == entity.Id);
        foreach (var pmt in payments)
        {
            transactions.Add(new TransactionSummaryDto
            {
                Id = pmt.Id, Type = TransactionType.ReceivePayment, TypeDisplay = "Payment",
                Date = pmt.PaymentDate, Amount = pmt.Amount, Status = pmt.Status
            });
        }

        Transactions = new ObservableCollection<TransactionSummaryDto>(
            transactions.OrderByDescending(t => t.Date));
    }

    protected override void NewEntity() => NavigationService.OpenForm("CustomerNew");
    protected override void EditEntity()
    {
        if (SelectedEntity != null)
            NavigationService.OpenForm("CustomerEdit", SelectedEntity.Id);
    }
}
