using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.Modules.Reports.ViewModels;

public partial class BalanceSheetReportViewModel : ReportViewModelBase
{
    private readonly IRepository<Account> _accountRepository;

    public BalanceSheetReportViewModel(IRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;
        Title = "Balance Sheet Standard";
    }

    protected override async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            var accounts = await _accountRepository.Query().OrderBy(a => a.SortOrder).ToListAsync();
            var rows = new ObservableCollection<ReportRowDto>();

            void AddSection(string label, AccountType[] types)
            {
                decimal sectionTotal = 0;
                rows.Add(new ReportRowDto { Label = label, IsBold = true, Level = 0 });
                foreach (var acc in accounts.Where(a => types.Contains(a.AccountType) && a.Balance != 0))
                {
                    rows.Add(new ReportRowDto { Label = $"  {acc.Number} {acc.Name}", Level = 1, Values = new() { ["Balance"] = acc.Balance } });
                    sectionTotal += acc.Balance;
                }
                rows.Add(new ReportRowDto { Label = $"Total {label}", IsBold = true, IsTotal = true, Values = new() { ["Balance"] = sectionTotal } });
            }

            // Assets
            rows.Add(new ReportRowDto { Label = "ASSETS", IsBold = true, Level = 0 });
            AddSection("Current Assets", new[] { AccountType.Bank, AccountType.AccountsReceivable, AccountType.OtherCurrentAsset });
            AddSection("Fixed Assets", new[] { AccountType.FixedAsset });
            AddSection("Other Assets", new[] { AccountType.OtherAsset });
            var totalAssets = accounts.Where(a => a.AccountType <= AccountType.OtherAsset).Sum(a => a.Balance);
            rows.Add(new ReportRowDto { Label = "TOTAL ASSETS", IsBold = true, IsTotal = true, IsSeparator = true, Values = new() { ["Balance"] = totalAssets } });

            // Liabilities
            rows.Add(new ReportRowDto { Label = "LIABILITIES & EQUITY", IsBold = true, Level = 0 });
            AddSection("Current Liabilities", new[] { AccountType.AccountsPayable, AccountType.CreditCard, AccountType.OtherCurrentLiability });
            AddSection("Long-Term Liabilities", new[] { AccountType.LongTermLiability });
            AddSection("Equity", new[] { AccountType.Equity });

            var totalLiabilitiesEquity = accounts.Where(a => a.AccountType >= AccountType.AccountsPayable && a.AccountType <= AccountType.Equity).Sum(a => a.Balance);
            rows.Add(new ReportRowDto { Label = "TOTAL LIABILITIES & EQUITY", IsBold = true, IsTotal = true, IsSeparator = true, Values = new() { ["Balance"] = totalLiabilitiesEquity } });

            Data = rows;
            HasData = true;
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
