using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Vendors;

namespace QBD.Modules.Reports.ViewModels;

public partial class VendorBalanceReportViewModel : ReportViewModelBase
{
    private readonly IRepository<Vendor> _vendorRepository;

    public VendorBalanceReportViewModel(IRepository<Vendor> vendorRepository)
    {
        _vendorRepository = vendorRepository;
        Title = "Vendor Balance Summary";
    }

    protected override async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            var vendors = await _vendorRepository.Query()
                .Where(v => v.IsActive && v.Balance != 0)
                .OrderBy(v => v.VendorName)
                .ToListAsync();

            var rows = new ObservableCollection<ReportRowDto>();
            decimal grandTotal = 0;

            foreach (var vendor in vendors)
            {
                rows.Add(new ReportRowDto
                {
                    Label = vendor.VendorName,
                    Level = 0,
                    EntityId = vendor.Id, EntityType = "Vendor",
                    Values = new() { ["Balance"] = vendor.Balance }
                });
                grandTotal += vendor.Balance;
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
