using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Vendors;
using QBD.Domain.Enums;

namespace QBD.Modules.Vendors.ViewModels;

public partial class VendorCreditFormViewModel : TransactionFormViewModelBase<VendorCredit, VendorCreditLine>
{
    private readonly IRepository<VendorCredit> _repository;
    private readonly IRepository<Vendor> _vendorRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly INumberSequenceService _numberSequenceService;

    [ObservableProperty] private ObservableCollection<Vendor> _vendors = new();
    [ObservableProperty] private ObservableCollection<Account> _expenseAccounts = new();

    public VendorCreditFormViewModel(
        IUnitOfWork unitOfWork, ITransactionPostingService postingService,
        INavigationService navigationService, IRepository<VendorCredit> repository,
        IRepository<Vendor> vendorRepository, IRepository<Account> accountRepository,
        INumberSequenceService numberSequenceService) : base(unitOfWork, postingService, navigationService)
    {
        _repository = repository; _vendorRepository = vendorRepository;
        _accountRepository = accountRepository; _numberSequenceService = numberSequenceService;
        Title = "Vendor Credit";
    }

    public override async Task InitializeAsync()
    {
        Vendors = new ObservableCollection<Vendor>(await _vendorRepository.Query().Where(v => v.IsActive).OrderBy(v => v.VendorName).ToListAsync());
        ExpenseAccounts = new ObservableCollection<Account>(await _accountRepository.Query().Where(a => a.AccountType == AccountType.Expense || a.AccountType == AccountType.CostOfGoodsSold).OrderBy(a => a.Number).ToListAsync());
        Header = new VendorCredit { RefNo = await _numberSequenceService.GetNextNumberAsync("VendorCredit"), Date = DateTime.Today, Status = DocStatus.Draft };
        Lines.Add(new VendorCreditLine());
    }

    protected override void RecalculateTotals()
    {
        GrandTotal = Lines.Sum(l => l.Amount);
        Header.Total = GrandTotal; Header.BalanceRemaining = GrandTotal;
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
            SetStatus("Vendor credit saved.");
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
            await PostingService.PostTransactionAsync(TransactionType.VendorCredit, Header.Id);
            Status = DocStatus.Posted; IsEditable = false;
        }
        catch (Exception ex) { SetError(ex.Message); }
    }

    protected override async Task VoidAsync()
    {
        if (Header.Id == 0) return;
        try { await PostingService.VoidTransactionAsync(TransactionType.VendorCredit, Header.Id); Status = DocStatus.Voided; IsEditable = false; }
        catch (Exception ex) { SetError(ex.Message); }
    }
}
