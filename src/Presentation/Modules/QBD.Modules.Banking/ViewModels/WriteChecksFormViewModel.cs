using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Banking;
using QBD.Domain.Enums;

namespace QBD.Modules.Banking.ViewModels;

public partial class WriteChecksFormViewModel : TransactionFormViewModelBase<Check, CheckExpenseLine>
{
    private readonly IRepository<Check> _checkRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly INumberSequenceService _numberSequenceService;

    [ObservableProperty] private ObservableCollection<Account> _bankAccounts = new();
    [ObservableProperty] private ObservableCollection<Account> _expenseAccounts = new();

    public WriteChecksFormViewModel(
        IUnitOfWork unitOfWork, ITransactionPostingService postingService,
        INavigationService navigationService, IRepository<Check> checkRepository,
        IRepository<Account> accountRepository, INumberSequenceService numberSequenceService)
        : base(unitOfWork, postingService, navigationService)
    {
        _checkRepository = checkRepository; _accountRepository = accountRepository;
        _numberSequenceService = numberSequenceService;
        Title = "Write Checks";
    }

    public override async Task InitializeAsync()
    {
        BankAccounts = new ObservableCollection<Account>(await _accountRepository.Query().Where(a => a.AccountType == AccountType.Bank).OrderBy(a => a.Name).ToListAsync());
        ExpenseAccounts = new ObservableCollection<Account>(await _accountRepository.Query().Where(a => a.AccountType == AccountType.Expense || a.AccountType == AccountType.CostOfGoodsSold || a.AccountType == AccountType.OtherExpense).OrderBy(a => a.Number).ToListAsync());
        Header = new Check { CheckNumber = await _numberSequenceService.GetNextNumberAsync("Check"), Date = DateTime.Today, Status = DocStatus.Draft };
        Lines.Add(new CheckExpenseLine());
    }

    protected override void RecalculateTotals()
    {
        GrandTotal = Lines.Sum(l => l.Amount);
        Header.Amount = GrandTotal;
    }

    protected override async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            Header.ExpenseLines = Lines.Where(l => l.Amount != 0).ToList();
            RecalculateTotals();
            if (Header.Id == 0) await _checkRepository.AddAsync(Header);
            else await _checkRepository.UpdateAsync(Header);
            await UnitOfWork.SaveChangesAsync();
            SetStatus($"Check {Header.CheckNumber} saved.");
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    protected override async Task SaveAndPostAsync()
    {
        await SaveAsync();
        if (ErrorMessage != null) return;
        try { await PostingService.PostTransactionAsync(TransactionType.Check, Header.Id); Status = DocStatus.Posted; IsEditable = false; SetStatus($"Check {Header.CheckNumber} posted."); }
        catch (Exception ex) { SetError(ex.Message); }
    }

    protected override async Task VoidAsync()
    {
        if (Header.Id == 0) return;
        try { await PostingService.VoidTransactionAsync(TransactionType.Check, Header.Id); Status = DocStatus.Voided; IsEditable = false; }
        catch (Exception ex) { SetError(ex.Message); }
    }
}
