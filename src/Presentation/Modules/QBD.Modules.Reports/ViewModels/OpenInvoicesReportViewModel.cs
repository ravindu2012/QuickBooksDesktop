using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Customers;
using QBD.Domain.Enums;

namespace QBD.Modules.Reports.ViewModels;

public partial class OpenInvoicesReportViewModel : ReportViewModelBase
{
    private readonly IRepository<Invoice> _invoiceRepository;

    public OpenInvoicesReportViewModel(IRepository<Invoice> invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
        Title = "Open Invoices";
    }

    protected override async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            var invoices = await _invoiceRepository.Query()
                .Include(i => i.Customer)
                .Where(i => i.BalanceDue > 0 && i.Status == DocStatus.Posted)
                .OrderBy(i => i.Customer.CustomerName).ThenBy(i => i.DueDate)
                .ToListAsync();

            var rows = new ObservableCollection<ReportRowDto>();
            decimal grandTotal = 0;
            int? currentCustomerId = null;
            decimal customerTotal = 0;

            foreach (var inv in invoices)
            {
                // Customer header when customer changes
                if (inv.CustomerId != currentCustomerId)
                {
                    // Add customer subtotal for previous customer
                    if (currentCustomerId != null && customerTotal != 0)
                    {
                        rows.Add(new ReportRowDto
                        {
                            Label = "  Subtotal", IsBold = true, IsTotal = true, Level = 1,
                            Values = new() { ["Open Balance"] = customerTotal }
                        });
                    }

                    currentCustomerId = inv.CustomerId;
                    customerTotal = 0;
                    rows.Add(new ReportRowDto
                    {
                        Label = inv.Customer.CustomerName,
                        IsBold = true, Level = 0,
                        EntityId = inv.CustomerId, EntityType = "Customer"
                    });
                }

                var daysOverdue = (ToDate - inv.DueDate).Days;
                rows.Add(new ReportRowDto
                {
                    Label = $"  {inv.InvoiceNumber}",
                    Level = 1,
                    EntityId = inv.Id, EntityType = "Invoice",
                    Values = new()
                    {
                        ["Date"] = inv.Date.ToString("MM/dd/yyyy"),
                        ["Due Date"] = inv.DueDate.ToString("MM/dd/yyyy"),
                        ["Original Amount"] = inv.Total,
                        ["Open Balance"] = inv.BalanceDue,
                        ["Overdue Days"] = daysOverdue > 0 ? (object)daysOverdue : null
                    }
                });

                customerTotal += inv.BalanceDue;
                grandTotal += inv.BalanceDue;
            }

            // Final customer subtotal
            if (currentCustomerId != null && customerTotal != 0)
            {
                rows.Add(new ReportRowDto
                {
                    Label = "  Subtotal", IsBold = true, IsTotal = true, Level = 1,
                    Values = new() { ["Open Balance"] = customerTotal }
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
