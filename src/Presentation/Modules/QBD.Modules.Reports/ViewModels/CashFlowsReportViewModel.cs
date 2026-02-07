using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.Modules.Reports.ViewModels;

public partial class CashFlowsReportViewModel : ReportViewModelBase
{
    private readonly IRepository<GLEntry> _glEntryRepository;
    private readonly IRepository<Account> _accountRepository;

    public CashFlowsReportViewModel(IRepository<GLEntry> glEntryRepository, IRepository<Account> accountRepository)
    {
        _glEntryRepository = glEntryRepository;
        _accountRepository = accountRepository;
        Title = "Statement of Cash Flows";
    }

    protected override async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            var accounts = await _accountRepository.Query().OrderBy(a => a.SortOrder).ToListAsync();

            var glEntries = await _glEntryRepository.Query()
                .Where(e => e.PostingDate >= FromDate && e.PostingDate <= ToDate && !e.IsVoid)
                .GroupBy(e => e.AccountId)
                .Select(g => new { AccountId = g.Key, Debits = g.Sum(e => e.DebitAmount), Credits = g.Sum(e => e.CreditAmount) })
                .ToListAsync();

            var rows = new ObservableCollection<ReportRowDto>();

            // Operating Activities
            rows.Add(new ReportRowDto { Label = "OPERATING ACTIVITIES", IsBold = true, Level = 0 });

            // Net Income (Income - COGS - Expenses)
            decimal netIncome = 0;
            foreach (var acc in accounts.Where(a => a.AccountType == AccountType.Income || a.AccountType == AccountType.OtherIncome))
            {
                var entry = glEntries.FirstOrDefault(e => e.AccountId == acc.Id);
                netIncome += (entry?.Credits ?? 0) - (entry?.Debits ?? 0);
            }
            foreach (var acc in accounts.Where(a => a.AccountType == AccountType.CostOfGoodsSold || a.AccountType == AccountType.Expense || a.AccountType == AccountType.OtherExpense))
            {
                var entry = glEntries.FirstOrDefault(e => e.AccountId == acc.Id);
                netIncome -= (entry?.Debits ?? 0) - (entry?.Credits ?? 0);
            }
            rows.Add(new ReportRowDto { Label = "  Net Income", Level = 1, Values = new() { ["Amount"] = netIncome } });

            // Adjustments: AR, AP, Other Current Assets/Liabilities changes
            decimal operatingAdjustments = 0;
            rows.Add(new ReportRowDto { Label = "  Adjustments to reconcile Net Income", IsBold = true, Level = 1 });

            foreach (var acc in accounts.Where(a => a.AccountType == AccountType.AccountsReceivable))
            {
                var entry = glEntries.FirstOrDefault(e => e.AccountId == acc.Id);
                var change = -((entry?.Debits ?? 0) - (entry?.Credits ?? 0));
                if (change != 0)
                {
                    operatingAdjustments += change;
                    rows.Add(new ReportRowDto { Label = $"    {acc.Name}", Level = 2, Values = new() { ["Amount"] = change } });
                }
            }
            foreach (var acc in accounts.Where(a => a.AccountType == AccountType.AccountsPayable || a.AccountType == AccountType.OtherCurrentLiability))
            {
                var entry = glEntries.FirstOrDefault(e => e.AccountId == acc.Id);
                var change = (entry?.Credits ?? 0) - (entry?.Debits ?? 0);
                if (change != 0)
                {
                    operatingAdjustments += change;
                    rows.Add(new ReportRowDto { Label = $"    {acc.Name}", Level = 2, Values = new() { ["Amount"] = change } });
                }
            }
            foreach (var acc in accounts.Where(a => a.AccountType == AccountType.OtherCurrentAsset))
            {
                var entry = glEntries.FirstOrDefault(e => e.AccountId == acc.Id);
                var change = -((entry?.Debits ?? 0) - (entry?.Credits ?? 0));
                if (change != 0)
                {
                    operatingAdjustments += change;
                    rows.Add(new ReportRowDto { Label = $"    {acc.Name}", Level = 2, Values = new() { ["Amount"] = change } });
                }
            }

            decimal netOperating = netIncome + operatingAdjustments;
            rows.Add(new ReportRowDto { Label = "Net cash provided by operating activities", IsBold = true, IsTotal = true, Values = new() { ["Amount"] = netOperating } });

            // Investing Activities
            rows.Add(new ReportRowDto { Label = "INVESTING ACTIVITIES", IsBold = true, Level = 0 });
            decimal netInvesting = 0;
            foreach (var acc in accounts.Where(a => a.AccountType == AccountType.FixedAsset || a.AccountType == AccountType.OtherAsset))
            {
                var entry = glEntries.FirstOrDefault(e => e.AccountId == acc.Id);
                var change = -((entry?.Debits ?? 0) - (entry?.Credits ?? 0));
                if (change != 0)
                {
                    netInvesting += change;
                    rows.Add(new ReportRowDto { Label = $"  {acc.Name}", Level = 1, Values = new() { ["Amount"] = change } });
                }
            }
            rows.Add(new ReportRowDto { Label = "Net cash used in investing activities", IsBold = true, IsTotal = true, Values = new() { ["Amount"] = netInvesting } });

            // Financing Activities
            rows.Add(new ReportRowDto { Label = "FINANCING ACTIVITIES", IsBold = true, Level = 0 });
            decimal netFinancing = 0;
            foreach (var acc in accounts.Where(a => a.AccountType == AccountType.LongTermLiability || a.AccountType == AccountType.Equity))
            {
                var entry = glEntries.FirstOrDefault(e => e.AccountId == acc.Id);
                var change = (entry?.Credits ?? 0) - (entry?.Debits ?? 0);
                if (change != 0)
                {
                    netFinancing += change;
                    rows.Add(new ReportRowDto { Label = $"  {acc.Name}", Level = 1, Values = new() { ["Amount"] = change } });
                }
            }
            rows.Add(new ReportRowDto { Label = "Net cash provided by financing activities", IsBold = true, IsTotal = true, Values = new() { ["Amount"] = netFinancing } });

            // Net change in cash
            decimal netChange = netOperating + netInvesting + netFinancing;
            rows.Add(new ReportRowDto { Label = "Net increase (decrease) in cash", IsBold = true, IsTotal = true, IsSeparator = true, Values = new() { ["Amount"] = netChange } });

            // Cash at beginning - sum of bank balances minus net change
            decimal cashEnd = accounts.Where(a => a.AccountType == AccountType.Bank).Sum(a => a.Balance);
            decimal cashBeginning = cashEnd - netChange;
            rows.Add(new ReportRowDto { Label = "Cash at beginning of period", Values = new() { ["Amount"] = cashBeginning } });
            rows.Add(new ReportRowDto { Label = "Cash at end of period", IsBold = true, IsTotal = true, IsSeparator = true, Values = new() { ["Amount"] = cashEnd } });

            Data = rows;
            HasData = rows.Count > 0;
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
