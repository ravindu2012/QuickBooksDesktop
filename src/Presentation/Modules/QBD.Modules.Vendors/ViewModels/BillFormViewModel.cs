using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Vendors;
using QBD.Domain.Entities.Items;
using QBD.Domain.Enums;

namespace QBD.Modules.Vendors.ViewModels;

public partial class BillFormViewModel : TransactionFormViewModelBase<Bill, BillExpenseLine>
{
    private readonly IRepository<Bill> _billRepository;
    private readonly IRepository<Vendor> _vendorRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Terms> _termsRepository;
    private readonly INumberSequenceService _numberSequenceService;

    [ObservableProperty] private ObservableCollection<Vendor> _vendors = new();
    [ObservableProperty] private ObservableCollection<Account> _expenseAccounts = new();
    [ObservableProperty] private ObservableCollection<Terms> _termsList = new();
    [ObservableProperty] private Vendor? _selectedVendor;

    public BillFormViewModel(
        IUnitOfWork unitOfWork, ITransactionPostingService postingService,
        INavigationService navigationService, IRepository<Bill> billRepository,
        IRepository<Vendor> vendorRepository, IRepository<Account> accountRepository,
        IRepository<Terms> termsRepository, INumberSequenceService numberSequenceService)
        : base(unitOfWork, postingService, navigationService)
    {
        _billRepository = billRepository; _vendorRepository = vendorRepository;
        _accountRepository = accountRepository; _termsRepository = termsRepository;
        _numberSequenceService = numberSequenceService;
        Title = "Enter Bills";
    }

    public override async Task InitializeAsync()
    {
        Vendors = new ObservableCollection<Vendor>(await _vendorRepository.Query().Where(v => v.IsActive).OrderBy(v => v.VendorName).ToListAsync());
        ExpenseAccounts = new ObservableCollection<Account>(await _accountRepository.Query()
            .Where(a => a.AccountType == AccountType.Expense || a.AccountType == AccountType.CostOfGoodsSold || a.AccountType == AccountType.OtherExpense)
            .OrderBy(a => a.Number).ToListAsync());
        TermsList = new ObservableCollection<Terms>(await _termsRepository.GetAllAsync());
        Header = new Bill
        {
            BillNumber = await _numberSequenceService.GetNextNumberAsync("Bill"),
            Date = DateTime.Today, DueDate = DateTime.Today.AddDays(30), Status = DocStatus.Draft
        };
        Lines.Add(new BillExpenseLine());
    }

    partial void OnSelectedVendorChanged(Vendor? value)
    {
        if (value != null)
        {
            Header.VendorId = value.Id;
            if (value.TermsId.HasValue)
            {
                var terms = TermsList.FirstOrDefault(t => t.Id == value.TermsId);
                if (terms != null) Header.DueDate = Header.Date.AddDays(terms.DueDays);
            }
        }
    }

    protected override void RecalculateTotals()
    {
        SubTotal = Lines.Sum(l => l.Amount);
        GrandTotal = SubTotal;
        Header.AmountDue = GrandTotal;
        Header.BalanceDue = GrandTotal;
    }

    protected override async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            Header.ExpenseLines = Lines.Where(l => l.Amount != 0).ToList();
            RecalculateTotals();
            if (Header.Id == 0) await _billRepository.AddAsync(Header);
            else await _billRepository.UpdateAsync(Header);
            await UnitOfWork.SaveChangesAsync();
            SetStatus($"Bill {Header.BillNumber} saved.");
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
            await PostingService.PostTransactionAsync(TransactionType.Bill, Header.Id);
            Status = DocStatus.Posted; IsEditable = false;
            SetStatus($"Bill {Header.BillNumber} posted to GL.");
        }
        catch (Exception ex) { SetError(ex.Message); }
    }

    protected override async Task VoidAsync()
    {
        if (Header.Id == 0) return;
        try { await PostingService.VoidTransactionAsync(TransactionType.Bill, Header.Id); Status = DocStatus.Voided; IsEditable = false; }
        catch (Exception ex) { SetError(ex.Message); }
    }
}
