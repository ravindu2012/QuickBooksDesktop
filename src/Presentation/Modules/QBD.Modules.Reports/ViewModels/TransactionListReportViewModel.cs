using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;

namespace QBD.Modules.Reports.ViewModels;

public partial class TransactionListReportViewModel : ReportViewModelBase
{
    private readonly IRepository<GLEntry> _glEntryRepository;

    public TransactionListReportViewModel(IRepository<GLEntry> glEntryRepository)
    {
        _glEntryRepository = glEntryRepository;
        Title = "Transaction List by Date";
    }

    protected override async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            var entries = await _glEntryRepository.Query()
                .Include(e => e.Account)
                .Where(e => e.PostingDate >= FromDate && e.PostingDate <= ToDate && !e.IsVoid)
                .OrderBy(e => e.PostingDate).ThenBy(e => e.TransactionType).ThenBy(e => e.TransactionNumber)
                .ToListAsync();

            var rows = new ObservableCollection<ReportRowDto>();
            decimal totalDebits = 0, totalCredits = 0;
            DateTime? currentDate = null;

            foreach (var entry in entries)
            {
                // Date header when date changes
                if (entry.PostingDate.Date != currentDate)
                {
                    currentDate = entry.PostingDate.Date;
                    rows.Add(new ReportRowDto
                    {
                        Label = currentDate.Value.ToString("dddd, MMMM dd, yyyy"),
                        IsBold = true, Level = 0
                    });
                }

                rows.Add(new ReportRowDto
                {
                    Label = $"  {entry.TransactionType}  {entry.TransactionNumber ?? ""}",
                    Level = 1,
                    EntityId = entry.TransactionId, EntityType = entry.TransactionType.ToString(),
                    Values = new()
                    {
                        ["Account"] = entry.Account != null ? $"{entry.Account.Number} {entry.Account.Name}" : "",
                        ["Name"] = entry.NameDisplay,
                        ["Memo"] = entry.Memo,
                        ["Debit"] = entry.DebitAmount != 0 ? (object)entry.DebitAmount : null,
                        ["Credit"] = entry.CreditAmount != 0 ? (object)entry.CreditAmount : null
                    }
                });

                totalDebits += entry.DebitAmount;
                totalCredits += entry.CreditAmount;
            }

            rows.Add(new ReportRowDto
            {
                Label = "TOTAL", IsBold = true, IsTotal = true, IsSeparator = true,
                Values = new() { ["Debit"] = totalDebits, ["Credit"] = totalCredits }
            });

            Data = rows;
            HasData = rows.Count > 0;
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
