using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Customers;

namespace QBD.Modules.Reports.ViewModels;

public partial class CustomerBalanceReportViewModel : ReportViewModelBase
{
    private readonly IRepository<Customer> _customerRepository;

    public CustomerBalanceReportViewModel(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
        Title = "Customer Balance Summary";
    }

    protected override async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            var customers = await _customerRepository.Query()
                .Where(c => c.IsActive && c.Balance != 0)
                .OrderBy(c => c.CustomerName)
                .ToListAsync();

            var rows = new ObservableCollection<ReportRowDto>();
            decimal grandTotal = 0;

            foreach (var customer in customers)
            {
                rows.Add(new ReportRowDto
                {
                    Label = customer.CustomerName,
                    Level = 0,
                    EntityId = customer.Id, EntityType = "Customer",
                    Values = new() { ["Balance"] = customer.Balance }
                });
                grandTotal += customer.Balance;
            }

            rows.Add(new ReportRowDto
            {
                Label = "TOTAL", IsBold = true, IsTotal = true, IsSeparator = true,
                Values = new() { ["Balance"] = grandTotal }
            });

            Data = rows;
            HasData = rows.Count > 0;
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
