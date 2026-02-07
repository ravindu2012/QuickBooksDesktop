using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace QBD.Modules.Reports.ViewModels;

public partial class ReportCenterViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public ReportCenterViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        Title = "Report Center";
    }

    [RelayCommand] private void OpenReport(string reportName) => _navigationService.OpenReport(reportName);
}
