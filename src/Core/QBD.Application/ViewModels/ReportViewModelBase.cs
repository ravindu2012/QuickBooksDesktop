using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace QBD.Application.ViewModels;

public abstract partial class ReportViewModelBase : ViewModelBase
{
    [ObservableProperty]
    private DateTime _fromDate = new DateTime(DateTime.Today.Year, 1, 1);

    [ObservableProperty]
    private DateTime _toDate = DateTime.Today;

    [ObservableProperty]
    private string _dateRange = "This Fiscal Year";

    [ObservableProperty]
    private string _reportBasis = "Accrual";

    [ObservableProperty]
    private ObservableCollection<ReportColumnDto> _columns = new();

    [ObservableProperty]
    private ObservableCollection<ReportRowDto> _data = new();

    [ObservableProperty]
    private bool _hasData;

    [RelayCommand]
    protected abstract Task GenerateReportAsync();

    [RelayCommand]
    protected virtual Task ExportToPdfAsync() => Task.CompletedTask;

    [RelayCommand]
    protected virtual Task ExportToExcelAsync() => Task.CompletedTask;

    [RelayCommand]
    protected virtual Task PrintAsync() => Task.CompletedTask;

    [RelayCommand]
    protected virtual void DrillDown(ReportRowDto row) { }

    [RelayCommand]
    private async Task RefreshReportAsync()
    {
        await GenerateReportAsync();
    }

    protected void SetDateRange(string range)
    {
        DateRange = range;
        var today = DateTime.Today;
        switch (range)
        {
            case "This Month":
                FromDate = new DateTime(today.Year, today.Month, 1);
                ToDate = today;
                break;
            case "This Quarter":
                var quarterStart = new DateTime(today.Year, ((today.Month - 1) / 3) * 3 + 1, 1);
                FromDate = quarterStart;
                ToDate = today;
                break;
            case "This Fiscal Year":
                FromDate = new DateTime(today.Year, 1, 1);
                ToDate = today;
                break;
            case "Last Month":
                FromDate = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                ToDate = new DateTime(today.Year, today.Month, 1).AddDays(-1);
                break;
            case "Last Fiscal Year":
                FromDate = new DateTime(today.Year - 1, 1, 1);
                ToDate = new DateTime(today.Year - 1, 12, 31);
                break;
        }
    }
}
