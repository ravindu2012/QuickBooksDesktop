using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.Modules.Banking.ViewModels;

public partial class BankRegisterViewModel : RegisterViewModelBase
{
    private readonly IRepository<GLEntry> _glEntryRepository;
    private readonly IRepository<Account> _accountRepository;

    [ObservableProperty] private ObservableCollection<Account> _bankAccounts = new();
    [ObservableProperty] private Account? _selectedAccount;

    public BankRegisterViewModel(
        IRepository<GLEntry> glEntryRepository,
        IRepository<Account> accountRepository)
    {
        _glEntryRepository = glEntryRepository;
        _accountRepository = accountRepository;
        Title = "Bank Register";
    }

    public override async Task InitializeAsync()
    {
        BankAccounts = new ObservableCollection<Account>(
            await _accountRepository.Query()
                .Where(a => a.AccountType == AccountType.Bank)
                .OrderBy(a => a.Name).ToListAsync());

        if (BankAccounts.Count > 0)
        {
            SelectedAccount = BankAccounts[0];
        }
    }

    partial void OnSelectedAccountChanged(Account? value)
    {
        if (value != null)
        {
            AccountId = value.Id;
            AccountName = value.Name;
            _ = LoadEntriesAsync();
        }
    }

    protected override async Task LoadEntriesAsync()
    {
        IsBusy = true;
        try
        {
            var query = _glEntryRepository.Query()
                .Where(e => e.AccountId == AccountId && !e.IsVoid);

            if (FromDate.HasValue)
                query = query.Where(e => e.PostingDate >= FromDate.Value);
            if (ToDate.HasValue)
                query = query.Where(e => e.PostingDate <= ToDate.Value);

            var entries = await query.OrderBy(e => e.PostingDate).ThenBy(e => e.Id).ToListAsync();

            decimal runningBalance = 0;
            var registerEntries = new ObservableCollection<RegisterEntryDto>();

            foreach (var entry in entries)
            {
                runningBalance += entry.DebitAmount - entry.CreditAmount;
                registerEntries.Add(new RegisterEntryDto
                {
                    Id = entry.Id,
                    Date = entry.PostingDate,
                    TransactionType = entry.TransactionType,
                    Number = entry.TransactionNumber ?? "",
                    Name = entry.NameDisplay,
                    Memo = entry.Memo,
                    Debit = entry.DebitAmount,
                    Credit = entry.CreditAmount,
                    RunningBalance = runningBalance,
                    TransactionId = entry.TransactionId
                });
            }

            Entries = registerEntries;
            EndingBalance = runningBalance;
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
