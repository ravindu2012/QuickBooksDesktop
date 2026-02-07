using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Customers;
using QBD.Domain.Entities.Items;
using QBD.Domain.Enums;

namespace QBD.Modules.Customers.ViewModels;

public partial class EstimateFormViewModel : TransactionFormViewModelBase<Estimate, EstimateLine>
{
    private readonly IRepository<Estimate> _repository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Item> _itemRepository;
    private readonly INumberSequenceService _numberSequenceService;

    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private ObservableCollection<Item> _items = new();

    public EstimateFormViewModel(
        IUnitOfWork unitOfWork, ITransactionPostingService postingService,
        INavigationService navigationService, IRepository<Estimate> repository,
        IRepository<Customer> customerRepository, IRepository<Item> itemRepository,
        INumberSequenceService numberSequenceService) : base(unitOfWork, postingService, navigationService)
    {
        _repository = repository; _customerRepository = customerRepository;
        _itemRepository = itemRepository; _numberSequenceService = numberSequenceService;
        Title = "Estimate";
    }

    public override async Task InitializeAsync()
    {
        Customers = new ObservableCollection<Customer>(await _customerRepository.Query().Where(c => c.IsActive).OrderBy(c => c.CustomerName).ToListAsync());
        Items = new ObservableCollection<Item>(await _itemRepository.Query().Where(i => i.IsActive).OrderBy(i => i.ItemName).ToListAsync());
        Header = new Estimate { EstimateNumber = await _numberSequenceService.GetNextNumberAsync("Estimate"), Date = DateTime.Today, Status = DocStatus.Draft };
        Lines.Add(new EstimateLine());
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
            SetStatus($"Estimate {Header.EstimateNumber} saved.");
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    // Estimates are non-posting
    protected override Task SaveAndPostAsync() => SaveAsync();

    protected override Task VoidAsync()
    {
        Header.Status = DocStatus.Voided;
        Status = DocStatus.Voided;
        IsEditable = false;
        return Task.CompletedTask;
    }
}
