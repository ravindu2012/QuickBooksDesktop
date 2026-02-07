using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;

namespace QBD.Modules.Reports.ViewModels;

public partial class GeneralLedgerReportViewModel : ReportViewModelBase
{
    private readonly IRepository<GLEntry> _glEntryRepository;
    private readonly IRepository<Account> _accountRepository;

    public GeneralLedgerReportViewModel(IRepository<GLEntry> glEntryRepository, IRepository<Account> accountRepository)
    {
        _glEntryRepository = glEntryRepository;
        _accountRepository = accountRepository;
        Title = "General Ledger";
    }

    protected override async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            var accounts = await _accountRepository.Query().OrderBy(a => a.SortOrder).ToListAsync();

            var entries = await _glEntryRepository.Query()
                .Where(e => e.PostingDate >= FromDate && e.PostingDate <= ToDate && !e.IsVoid)
                .OrderBy(e => e.PostingDate)
                .ToListAsync();

            var rows = new ObservableCollection<ReportRowDto>();

            foreach (var account in accounts)
            {
                var accountEntries = entries.Where(e => e.AccountId == account.Id).ToList();
                if (accountEntries.Count == 0) continue;

                rows.Add(new ReportRowDto
                {
                    Label = $"{account.Number} {account.Name}",
                    IsBold = true, Level = 0,
                    EntityId = account.Id, EntityType = "Account"
                });

                decimal runningBalance = 0;
                foreach (var entry in accountEntries)
                {
                    runningBalance += entry.DebitAmount - entry.CreditAmount;
                    rows.Add(new ReportRowDto
                    {
                        Label = $"  {entry.PostingDate:MM/dd/yyyy}  {entry.TransactionType}  {entry.TransactionNumber ?? ""}  {entry.NameDisplay ?? ""}",
                        Level = 1,
                        Values = new()
                        {
                            ["Debit"] = entry.DebitAmount != 0 ? (object)entry.DebitAmount : null,
                            ["Credit"] = entry.CreditAmount != 0 ? (object)entry.CreditAmount : null,
                            ["Balance"] = runningBalance,
                            ["Memo"] = entry.Memo
                        },
                        EntityId = entry.TransactionId, EntityType = entry.TransactionType.ToString()
                    });
                }

                decimal totalDebits = accountEntries.Sum(e => e.DebitAmount);
                decimal totalCredits = accountEntries.Sum(e => e.CreditAmount);
                rows.Add(new ReportRowDto
                {
                    Label = $"  Total {account.Name}",
                    IsBold = true, IsTotal = true, Level = 1,
                    Values = new() { ["Debit"] = totalDebits, ["Credit"] = totalCredits, ["Balance"] = runningBalance }
                });
            }

            Data = rows;
            HasData = rows.Count > 0;
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
