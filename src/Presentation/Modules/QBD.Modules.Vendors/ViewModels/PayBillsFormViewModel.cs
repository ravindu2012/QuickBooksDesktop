using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Vendors;
using QBD.Domain.Enums;

namespace QBD.Modules.Vendors.ViewModels;

public partial class PayBillsFormViewModel : TransactionFormViewModelBase<BillPayment, BillPaymentApplication>
{
    private readonly IRepository<BillPayment> _paymentRepository;
    private readonly IRepository<Bill> _billRepository;
    private readonly IRepository<Account> _accountRepository;

    [ObservableProperty] private ObservableCollection<Bill> _unpaidBills = new();
    [ObservableProperty] private ObservableCollection<Account> _paymentAccounts = new();

    public PayBillsFormViewModel(
        IUnitOfWork unitOfWork, ITransactionPostingService postingService,
        INavigationService navigationService, IRepository<BillPayment> paymentRepository,
        IRepository<Bill> billRepository, IRepository<Account> accountRepository)
        : base(unitOfWork, postingService, navigationService)
    {
        _paymentRepository = paymentRepository; _billRepository = billRepository;
        _accountRepository = accountRepository;
        Title = "Pay Bills";
    }

    public override async Task InitializeAsync()
    {
        PaymentAccounts = new ObservableCollection<Account>(await _accountRepository.Query().Where(a => a.AccountType == AccountType.Bank).OrderBy(a => a.Name).ToListAsync());
        var bills = await _billRepository.Query().Include(b => b.Vendor).Where(b => b.BalanceDue > 0 && b.Status == DocStatus.Posted).ToListAsync();
        UnpaidBills = new ObservableCollection<Bill>(bills);
        Header = new BillPayment { Date = DateTime.Today, Status = DocStatus.Draft };

        Lines.Clear();
        foreach (var bill in bills)
        {
            Lines.Add(new BillPaymentApplication { BillId = bill.Id, AmountApplied = bill.BalanceDue });
        }
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
            SetStatus("Bill payment saved.");
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
            await PostingService.PostTransactionAsync(TransactionType.BillPayment, Header.Id);
            Status = DocStatus.Posted; IsEditable = false;
            SetStatus("Bill payment posted to GL.");
        }
        catch (Exception ex) { SetError(ex.Message); }
    }

    protected override async Task VoidAsync()
    {
        if (Header.Id == 0) return;
        try { await PostingService.VoidTransactionAsync(TransactionType.BillPayment, Header.Id); Status = DocStatus.Voided; IsEditable = false; }
        catch (Exception ex) { SetError(ex.Message); }
    }
}
