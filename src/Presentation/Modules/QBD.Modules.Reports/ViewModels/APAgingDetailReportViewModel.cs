using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Vendors;
using QBD.Domain.Enums;

namespace QBD.Modules.Reports.ViewModels;

public partial class APAgingDetailReportViewModel : ReportViewModelBase
{
    private readonly IRepository<Bill> _billRepository;
    private readonly IRepository<Vendor> _vendorRepository;

    public APAgingDetailReportViewModel(IRepository<Bill> billRepository, IRepository<Vendor> vendorRepository)
    {
        _billRepository = billRepository;
        _vendorRepository = vendorRepository;
        Title = "A/P Aging Detail";
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
                .OrderBy(b => b.Vendor.VendorName).ThenBy(b => b.DueDate)
                .ToListAsync();

            var rows = new ObservableCollection<ReportRowDto>();
            decimal grandTotal = 0;
            int? currentVendorId = null;

            foreach (var bill in bills)
            {
                // Vendor header
                if (bill.VendorId != currentVendorId)
                {
                    currentVendorId = bill.VendorId;
                    rows.Add(new ReportRowDto
                    {
                        Label = bill.Vendor.VendorName,
                        IsBold = true, Level = 0,
                        EntityId = bill.VendorId, EntityType = "Vendor"
                    });
                }

                var daysOverdue = (today - bill.DueDate).Days;
                var agingBucket = daysOverdue <= 0 ? "Current"
                    : daysOverdue <= 30 ? "1-30"
                    : daysOverdue <= 60 ? "31-60"
                    : daysOverdue <= 90 ? "61-90"
                    : "90+";

                rows.Add(new ReportRowDto
                {
                    Label = $"  {bill.BillNumber ?? bill.VendorRefNo ?? $"Bill #{bill.Id}"}",
                    Level = 1,
                    EntityId = bill.Id, EntityType = "Bill",
                    Values = new()
                    {
                        ["Date"] = bill.Date.ToString("MM/dd/yyyy"),
                        ["Due Date"] = bill.DueDate.ToString("MM/dd/yyyy"),
                        ["Aging"] = agingBucket,
                        ["Open Balance"] = bill.BalanceDue
                    }
                });

                grandTotal += bill.BalanceDue;
            }

            rows.Add(new ReportRowDto
            {
                Label = "TOTAL", IsBold = true, IsTotal = true, IsSeparator = true,
                Values = new() { ["Open Balance"] = grandTotal }
            });

            Data = rows;
            HasData = rows.Count > 0;
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
