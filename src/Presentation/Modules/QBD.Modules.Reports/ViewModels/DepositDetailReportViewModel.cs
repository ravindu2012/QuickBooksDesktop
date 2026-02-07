using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Banking;
using QBD.Domain.Enums;

namespace QBD.Modules.Reports.ViewModels;

public partial class DepositDetailReportViewModel : ReportViewModelBase
{
    private readonly IRepository<Deposit> _depositRepository;

    public DepositDetailReportViewModel(IRepository<Deposit> depositRepository)
    {
        _depositRepository = depositRepository;
        Title = "Deposit Detail";
    }

    protected override async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            var deposits = await _depositRepository.Query()
                .Include(d => d.BankAccount)
                .Include(d => d.Lines).ThenInclude(l => l.FromAccount)
                .Where(d => d.Date >= FromDate && d.Date <= ToDate && d.Status == DocStatus.Posted)
                .OrderBy(d => d.Date)
                .ToListAsync();

            var rows = new ObservableCollection<ReportRowDto>();
            decimal grandTotal = 0;

            foreach (var deposit in deposits)
            {
                // Deposit header
                rows.Add(new ReportRowDto
                {
                    Label = $"Deposit {deposit.Date:MM/dd/yyyy} - {deposit.BankAccount?.Name ?? "Unknown Account"}",
                    IsBold = true, Level = 0,
                    EntityId = deposit.Id, EntityType = "Deposit",
                    Values = new() { ["Total"] = deposit.Total }
                });

                // Deposit lines
                foreach (var line in deposit.Lines)
                {
                    rows.Add(new ReportRowDto
                    {
                        Label = $"  {line.ReceivedFrom ?? ""}",
                        Level = 1,
                        Values = new()
                        {
                            ["From Account"] = line.FromAccount != null ? $"{line.FromAccount.Number} {line.FromAccount.Name}" : "",
                            ["Memo"] = line.Memo,
                            ["Amount"] = line.Amount
                        }
                    });
                }

                // Deposit subtotal
                rows.Add(new ReportRowDto
                {
                    Label = "  Deposit Total", IsBold = true, IsTotal = true, Level = 1,
                    Values = new() { ["Amount"] = deposit.Total }
                });

                grandTotal += deposit.Total;
            }

            rows.Add(new ReportRowDto
            {
                Label = "GRAND TOTAL", IsBold = true, IsTotal = true, IsSeparator = true,
                Values = new() { ["Amount"] = grandTotal }
            });

            Data = rows;
            HasData = rows.Count > 0;
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
