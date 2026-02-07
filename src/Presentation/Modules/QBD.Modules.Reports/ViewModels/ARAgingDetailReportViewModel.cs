using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Customers;
using QBD.Domain.Enums;

namespace QBD.Modules.Reports.ViewModels;

public partial class ARAgingDetailReportViewModel : ReportViewModelBase
{
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<Customer> _customerRepository;

    public ARAgingDetailReportViewModel(IRepository<Invoice> invoiceRepository, IRepository<Customer> customerRepository)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        Title = "A/R Aging Detail";
    }

    protected override async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            var today = ToDate;
            var invoices = await _invoiceRepository.Query()
                .Include(i => i.Customer)
                .Where(i => i.BalanceDue > 0 && i.Status == DocStatus.Posted)
                .OrderBy(i => i.Customer.CustomerName).ThenBy(i => i.DueDate)
                .ToListAsync();

            var rows = new ObservableCollection<ReportRowDto>();
            decimal grandTotal = 0;
            int? currentCustomerId = null;

            foreach (var inv in invoices)
            {
                // Customer header
                if (inv.CustomerId != currentCustomerId)
                {
                    currentCustomerId = inv.CustomerId;
                    rows.Add(new ReportRowDto
                    {
                        Label = inv.Customer.CustomerName,
                        IsBold = true, Level = 0,
                        EntityId = inv.CustomerId, EntityType = "Customer"
                    });
                }

                var daysOverdue = (today - inv.DueDate).Days;
                var agingBucket = daysOverdue <= 0 ? "Current"
                    : daysOverdue <= 30 ? "1-30"
                    : daysOverdue <= 60 ? "31-60"
                    : daysOverdue <= 90 ? "61-90"
                    : "90+";

                rows.Add(new ReportRowDto
                {
                    Label = $"  {inv.InvoiceNumber}",
                    Level = 1,
                    EntityId = inv.Id, EntityType = "Invoice",
                    Values = new()
                    {
                        ["Date"] = inv.Date.ToString("MM/dd/yyyy"),
                        ["Due Date"] = inv.DueDate.ToString("MM/dd/yyyy"),
                        ["Aging"] = agingBucket,
                        ["Open Balance"] = inv.BalanceDue
                    }
                });

                grandTotal += inv.BalanceDue;
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
