using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Vendors;
using QBD.Domain.Enums;

namespace QBD.Modules.Reports.ViewModels;

public partial class UnpaidBillsReportViewModel : ReportViewModelBase
{
    private readonly IRepository<Bill> _billRepository;

    public UnpaidBillsReportViewModel(IRepository<Bill> billRepository)
    {
        _billRepository = billRepository;
        Title = "Unpaid Bills Detail";
    }

    protected override async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            var bills = await _billRepository.Query()
                .Include(b => b.Vendor)
                .Where(b => b.BalanceDue > 0 && b.Status == DocStatus.Posted)
                .OrderBy(b => b.Vendor.VendorName).ThenBy(b => b.DueDate)
                .ToListAsync();

            var rows = new ObservableCollection<ReportRowDto>();
            decimal grandTotal = 0;
            int? currentVendorId = null;
            decimal vendorTotal = 0;

            foreach (var bill in bills)
            {
                // Vendor header when vendor changes
                if (bill.VendorId != currentVendorId)
                {
                    // Add vendor subtotal for previous vendor
                    if (currentVendorId != null && vendorTotal != 0)
                    {
                        rows.Add(new ReportRowDto
                        {
                            Label = "  Subtotal", IsBold = true, IsTotal = true, Level = 1,
                            Values = new() { ["Open Balance"] = vendorTotal }
                        });
                    }

                    currentVendorId = bill.VendorId;
                    vendorTotal = 0;
                    rows.Add(new ReportRowDto
                    {
                        Label = bill.Vendor.VendorName,
                        IsBold = true, Level = 0,
                        EntityId = bill.VendorId, EntityType = "Vendor"
                    });
                }

                var daysOverdue = (ToDate - bill.DueDate).Days;
                rows.Add(new ReportRowDto
                {
                    Label = $"  {bill.BillNumber ?? bill.VendorRefNo ?? $"Bill #{bill.Id}"}",
                    Level = 1,
                    EntityId = bill.Id, EntityType = "Bill",
                    Values = new()
                    {
                        ["Date"] = bill.Date.ToString("MM/dd/yyyy"),
                        ["Due Date"] = bill.DueDate.ToString("MM/dd/yyyy"),
                        ["Original Amount"] = bill.AmountDue,
                        ["Open Balance"] = bill.BalanceDue,
                        ["Overdue Days"] = daysOverdue > 0 ? (object)daysOverdue : null
                    }
                });

                vendorTotal += bill.BalanceDue;
                grandTotal += bill.BalanceDue;
            }

            // Final vendor subtotal
            if (currentVendorId != null && vendorTotal != 0)
            {
                rows.Add(new ReportRowDto
                {
                    Label = "  Subtotal", IsBold = true, IsTotal = true, Level = 1,
                    Values = new() { ["Open Balance"] = vendorTotal }
                });
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
