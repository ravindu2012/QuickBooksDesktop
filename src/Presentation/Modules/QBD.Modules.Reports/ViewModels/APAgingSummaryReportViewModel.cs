using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Vendors;
using QBD.Domain.Enums;

namespace QBD.Modules.Reports.ViewModels;

public partial class APAgingSummaryReportViewModel : ReportViewModelBase
{
    private readonly IRepository<Bill> _billRepository;
    private readonly IRepository<Vendor> _vendorRepository;

    public APAgingSummaryReportViewModel(IRepository<Bill> billRepository, IRepository<Vendor> vendorRepository)
    {
        _billRepository = billRepository;
        _vendorRepository = vendorRepository;
        Title = "A/P Aging Summary";
    }

    protected override async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            var today = ToDate;
            var bills = await _billRepository.Query()
                .Include(b => b.Vendor)
                .Where(b => b.BalanceDue > 0 && b.Status == DocStatus.Posted)
                .ToListAsync();

            var rows = new ObservableCollection<ReportRowDto>();
            decimal totalCurrent = 0, total1to30 = 0, total31to60 = 0, total61to90 = 0, totalOver90 = 0;

            var grouped = bills.GroupBy(b => b.VendorId).OrderBy(g => g.First().Vendor.VendorName);
            foreach (var group in grouped)
            {
                var vendor = group.First().Vendor;
                decimal current = 0, days1to30 = 0, days31to60 = 0, days61to90 = 0, over90 = 0;

                foreach (var bill in group)
                {
                    var daysOverdue = (today - bill.DueDate).Days;
                    if (daysOverdue <= 0) current += bill.BalanceDue;
                    else if (daysOverdue <= 30) days1to30 += bill.BalanceDue;
                    else if (daysOverdue <= 60) days31to60 += bill.BalanceDue;
                    else if (daysOverdue <= 90) days61to90 += bill.BalanceDue;
                    else over90 += bill.BalanceDue;
                }

                var total = current + days1to30 + days31to60 + days61to90 + over90;
                rows.Add(new ReportRowDto
                {
                    Label = vendor.VendorName,
                    Level = 0,
                    EntityId = vendor.Id, EntityType = "Vendor",
                    Values = new()
                    {
                        ["Current"] = current, ["1-30"] = days1to30, ["31-60"] = days31to60,
                        ["61-90"] = days61to90, ["90+"] = over90, ["Total"] = total
                    }
                });

                totalCurrent += current; total1to30 += days1to30; total31to60 += days31to60;
                total61to90 += days61to90; totalOver90 += over90;
            }

            rows.Add(new ReportRowDto
            {
                Label = "TOTAL", IsBold = true, IsTotal = true,
                Values = new()
                {
                    ["Current"] = totalCurrent, ["1-30"] = total1to30, ["31-60"] = total31to60,
                    ["61-90"] = total61to90, ["90+"] = totalOver90,
                    ["Total"] = totalCurrent + total1to30 + total31to60 + total61to90 + totalOver90
                }
            });

            Data = rows;
            HasData = rows.Count > 0;
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
