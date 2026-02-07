using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Vendors;
using QBD.Domain.Entities.Items;
using QBD.Domain.Enums;

namespace QBD.Modules.Vendors.ViewModels;

public partial class PurchaseOrderFormViewModel : TransactionFormViewModelBase<PurchaseOrder, PurchaseOrderLine>
{
    private readonly IRepository<PurchaseOrder> _repository;
    private readonly IRepository<Vendor> _vendorRepository;
    private readonly IRepository<Item> _itemRepository;
    private readonly INumberSequenceService _numberSequenceService;

    [ObservableProperty] private ObservableCollection<Vendor> _vendors = new();
    [ObservableProperty] private ObservableCollection<Item> _items = new();

    public PurchaseOrderFormViewModel(
        IUnitOfWork unitOfWork, ITransactionPostingService postingService,
        INavigationService navigationService, IRepository<PurchaseOrder> repository,
        IRepository<Vendor> vendorRepository, IRepository<Item> itemRepository,
        INumberSequenceService numberSequenceService) : base(unitOfWork, postingService, navigationService)
    {
        _repository = repository; _vendorRepository = vendorRepository;
        _itemRepository = itemRepository; _numberSequenceService = numberSequenceService;
        Title = "Purchase Order";
    }

    public override async Task InitializeAsync()
    {
        Vendors = new ObservableCollection<Vendor>(await _vendorRepository.Query().Where(v => v.IsActive).OrderBy(v => v.VendorName).ToListAsync());
        Items = new ObservableCollection<Item>(await _itemRepository.Query().Where(i => i.IsActive).OrderBy(i => i.ItemName).ToListAsync());
        Header = new PurchaseOrder { PONumber = await _numberSequenceService.GetNextNumberAsync("PurchaseOrder"), Date = DateTime.Today, Status = DocStatus.Draft };
        Lines.Add(new PurchaseOrderLine());
    }

    protected override void RecalculateTotals()
    {
        SubTotal = Lines.Sum(l => l.Amount);
        GrandTotal = SubTotal;
        Header.Subtotal = SubTotal; Header.Total = GrandTotal;
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
            SetStatus($"PO {Header.PONumber} saved.");
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    // POs are non-posting
    protected override Task SaveAndPostAsync() => SaveAsync();
    protected override Task VoidAsync() { Header.Status = DocStatus.Voided; Status = DocStatus.Voided; IsEditable = false; return Task.CompletedTask; }
}
