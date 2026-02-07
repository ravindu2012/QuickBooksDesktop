using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;

namespace QBD.Modules.Reports.ViewModels;

public partial class TrialBalanceReportViewModel : ReportViewModelBase
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<GLEntry> _glEntryRepository;

    public TrialBalanceReportViewModel(IRepository<Account> accountRepository, IRepository<GLEntry> glEntryRepository)
    {
        _accountRepository = accountRepository;
        _glEntryRepository = glEntryRepository;
        Title = "Trial Balance";
    }

    protected override async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            var entries = await _glEntryRepository.Query()
                .Where(e => e.PostingDate >= FromDate && e.PostingDate <= ToDate && !e.IsVoid)
                .GroupBy(e => e.AccountId)
                .Select(g => new { AccountId = g.Key, Debits = g.Sum(e => e.DebitAmount), Credits = g.Sum(e => e.CreditAmount) })
                .ToListAsync();

            var accounts = await _accountRepository.Query().OrderBy(a => a.SortOrder).ToListAsync();
            var rows = new ObservableCollection<ReportRowDto>();
            decimal totalDebits = 0, totalCredits = 0;

            foreach (var account in accounts)
            {
                var entry = entries.FirstOrDefault(e => e.AccountId == account.Id);
                if (entry == null) continue;
                totalDebits += entry.Debits;
                totalCredits += entry.Credits;
                rows.Add(new ReportRowDto
                {
                    Label = $"{account.Number} {account.Name}",
                    Values = new() { ["Debit"] = entry.Debits, ["Credit"] = entry.Credits },
                    EntityId = account.Id, EntityType = "Account"
                });
            }

            rows.Add(new ReportRowDto
            {
                Label = "TOTAL", IsBold = true, IsTotal = true,
                Values = new() { ["Debit"] = totalDebits, ["Credit"] = totalCredits }
            });

            Data = rows;
            HasData = true;
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
