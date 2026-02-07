using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Vendors;
using QBD.Domain.Enums;

namespace QBD.Modules.Vendors.ViewModels;

public partial class VendorCenterViewModel : CenterViewModelBase<Vendor, Vendor>
{
    private readonly IRepository<Vendor> _vendorRepository;
    private readonly IRepository<Bill> _billRepository;

    public VendorCenterViewModel(
        INavigationService navigationService,
        IRepository<Vendor> vendorRepository,
        IRepository<Bill> billRepository) : base(navigationService)
    {
        _vendorRepository = vendorRepository;
        _billRepository = billRepository;
        Title = "Vendor Center";
    }

    public override async Task InitializeAsync() => await LoadEntitiesAsync();

    protected override async Task LoadEntitiesAsync()
    {
        var query = _vendorRepository.Query();
        if (!string.IsNullOrWhiteSpace(FilterText))
            query = query.Where(v => v.VendorName.Contains(FilterText));
        var vendors = await query.OrderBy(v => v.VendorName).ToListAsync();
        EntityList = new ObservableCollection<Vendor>(vendors);
    }

    protected override Task LoadEntityDetailAsync(Vendor entity) { EntityDetail = entity; return Task.CompletedTask; }

    protected override async Task LoadTransactionsAsync(Vendor entity)
    {
        var transactions = new List<TransactionSummaryDto>();
        var bills = await _billRepository.FindAsync(b => b.VendorId == entity.Id);
        foreach (var bill in bills)
        {
            transactions.Add(new TransactionSummaryDto
            {
                Id = bill.Id, Type = TransactionType.Bill, TypeDisplay = "Bill",
                Number = bill.BillNumber ?? "", Date = bill.Date, Amount = bill.AmountDue,
                Balance = bill.BalanceDue, Status = bill.Status
            });
        }
        Transactions = new ObservableCollection<TransactionSummaryDto>(transactions.OrderByDescending(t => t.Date));
    }
}
