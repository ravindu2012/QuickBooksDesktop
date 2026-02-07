using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Banking;

namespace QBD.Modules.Banking.ViewModels;

public partial class ReconcileViewModel : ViewModelBase
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<GLEntry> _glEntryRepository;
    private readonly IRepository<Reconciliation> _reconciliationRepository;
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty] private ObservableCollection<Account> _bankAccounts = new();
    [ObservableProperty] private Account? _selectedAccount;
    [ObservableProperty] private DateTime _statementDate = DateTime.Today;
    [ObservableProperty] private decimal _statementEndingBalance;
    [ObservableProperty] private decimal _clearedBalance;
    [ObservableProperty] private decimal _difference;
    [ObservableProperty] private ObservableCollection<ReconcileEntryDto> _entries = new();

    public ReconcileViewModel(
        IRepository<Account> accountRepository,
        IRepository<GLEntry> glEntryRepository,
        IRepository<Reconciliation> reconciliationRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _glEntryRepository = glEntryRepository;
        _reconciliationRepository = reconciliationRepository;
        _unitOfWork = unitOfWork;
        Title = "Reconcile";
    }

    public override async Task InitializeAsync()
    {
        BankAccounts = new ObservableCollection<Account>(
            await _accountRepository.Query()
                .Where(a => a.AccountType == Domain.Enums.AccountType.Bank)
                .OrderBy(a => a.Name).ToListAsync());
    }

    [RelayCommand]
    private async Task LoadTransactionsAsync()
    {
        if (SelectedAccount == null) return;

        var glEntries = await _glEntryRepository.Query()
            .Where(e => e.AccountId == SelectedAccount.Id && !e.IsVoid && e.PostingDate <= StatementDate)
            .OrderBy(e => e.PostingDate).ToListAsync();

        var reconcileEntries = glEntries.Select(e => new ReconcileEntryDto
        {
            GLEntryId = e.Id,
            Date = e.PostingDate,
            Type = e.TransactionType.ToString(),
            Number = e.TransactionNumber ?? "",
            Name = e.NameDisplay ?? "",
            Amount = e.DebitAmount - e.CreditAmount,
            IsCleared = false
        }).ToList();

        Entries = new ObservableCollection<ReconcileEntryDto>(reconcileEntries);
        UpdateDifference();
    }

    public void UpdateDifference()
    {
        ClearedBalance = Entries.Where(e => e.IsCleared).Sum(e => e.Amount);
        Difference = StatementEndingBalance - ClearedBalance;
    }

    [RelayCommand]
    private async Task FinishReconcileAsync()
    {
        if (Difference != 0)
        {
            SetError("Difference must be $0.00 to complete reconciliation.");
            return;
        }

        var reconciliation = new Reconciliation
        {
            AccountId = SelectedAccount!.Id,
            StatementDate = StatementDate,
            EndingBalance = StatementEndingBalance,
            IsCompleted = true
        };
        await _reconciliationRepository.AddAsync(reconciliation);
        await _unitOfWork.SaveChangesAsync();
        SetStatus("Reconciliation completed!");
    }
}

public class ReconcileEntryDto
{
    public int GLEntryId { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsCleared { get; set; }
}
