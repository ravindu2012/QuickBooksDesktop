using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.Modules.Reports.ViewModels;

public partial class ProfitLossReportViewModel : ReportViewModelBase
{
    private readonly IRepository<GLEntry> _glEntryRepository;
    private readonly IRepository<Account> _accountRepository;

    public ProfitLossReportViewModel(IRepository<GLEntry> glEntryRepository, IRepository<Account> accountRepository)
    {
        _glEntryRepository = glEntryRepository;
        _accountRepository = accountRepository;
        Title = "Profit & Loss Standard";
    }

    protected override async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            var accounts = await _accountRepository.Query()
                .Where(a => a.AccountType == AccountType.Income || a.AccountType == AccountType.OtherIncome ||
                           a.AccountType == AccountType.CostOfGoodsSold || a.AccountType == AccountType.Expense ||
                           a.AccountType == AccountType.OtherExpense)
                .OrderBy(a => a.SortOrder).ToListAsync();

            var glEntries = await _glEntryRepository.Query()
                .Where(e => e.PostingDate >= FromDate && e.PostingDate <= ToDate && !e.IsVoid)
                .GroupBy(e => e.AccountId)
                .Select(g => new { AccountId = g.Key, Debits = g.Sum(e => e.DebitAmount), Credits = g.Sum(e => e.CreditAmount) })
                .ToListAsync();

            var rows = new ObservableCollection<ReportRowDto>();
            decimal totalIncome = 0, totalCOGS = 0, totalExpense = 0;

            // Income section
            rows.Add(new ReportRowDto { Label = "Income", IsBold = true, Level = 0 });
            foreach (var account in accounts.Where(a => a.AccountType == AccountType.Income || a.AccountType == AccountType.OtherIncome))
            {
                var entry = glEntries.FirstOrDefault(e => e.AccountId == account.Id);
                var amount = (entry?.Credits ?? 0) - (entry?.Debits ?? 0);
                if (amount != 0)
                {
                    totalIncome += amount;
                    rows.Add(new ReportRowDto { Label = $"  {account.Number} {account.Name}", Level = 1, Values = new() { ["Amount"] = amount } });
                }
            }
            rows.Add(new ReportRowDto { Label = "Total Income", IsBold = true, IsTotal = true, Values = new() { ["Amount"] = totalIncome } });

            // COGS section
            rows.Add(new ReportRowDto { Label = "Cost of Goods Sold", IsBold = true, Level = 0 });
            foreach (var account in accounts.Where(a => a.AccountType == AccountType.CostOfGoodsSold))
            {
                var entry = glEntries.FirstOrDefault(e => e.AccountId == account.Id);
                var amount = (entry?.Debits ?? 0) - (entry?.Credits ?? 0);
                if (amount != 0)
                {
                    totalCOGS += amount;
                    rows.Add(new ReportRowDto { Label = $"  {account.Number} {account.Name}", Level = 1, Values = new() { ["Amount"] = amount } });
                }
            }
            rows.Add(new ReportRowDto { Label = "Total COGS", IsBold = true, IsTotal = true, Values = new() { ["Amount"] = totalCOGS } });

            // Gross Profit
            rows.Add(new ReportRowDto { Label = "Gross Profit", IsBold = true, IsTotal = true, Values = new() { ["Amount"] = totalIncome - totalCOGS }, IsSeparator = true });

            // Expense section
            rows.Add(new ReportRowDto { Label = "Expenses", IsBold = true, Level = 0 });
            foreach (var account in accounts.Where(a => a.AccountType == AccountType.Expense || a.AccountType == AccountType.OtherExpense))
            {
                var entry = glEntries.FirstOrDefault(e => e.AccountId == account.Id);
                var amount = (entry?.Debits ?? 0) - (entry?.Credits ?? 0);
                if (amount != 0)
                {
                    totalExpense += amount;
                    rows.Add(new ReportRowDto { Label = $"  {account.Number} {account.Name}", Level = 1, Values = new() { ["Amount"] = amount } });
                }
            }
            rows.Add(new ReportRowDto { Label = "Total Expenses", IsBold = true, IsTotal = true, Values = new() { ["Amount"] = totalExpense } });

            // Net Income
            rows.Add(new ReportRowDto { Label = "Net Income", IsBold = true, IsTotal = true, Values = new() { ["Amount"] = totalIncome - totalCOGS - totalExpense }, IsSeparator = true });

            Data = rows;
            HasData = rows.Count > 0;
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
