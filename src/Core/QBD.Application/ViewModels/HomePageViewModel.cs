using CommunityToolkit.Mvvm.Input;
using QBD.Application.Interfaces;

namespace QBD.Application.ViewModels;

public partial class HomePageViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public HomePageViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        Title = "Home Page";
    }

    [RelayCommand]
    private void NavigateToForm(string formName)
    {
        _navigationService.OpenForm(formName);
    }

    [RelayCommand]
    private void NavigateToCenter(string centerName)
    {
        _navigationService.OpenCenter(centerName);
    }

    [RelayCommand]
    private void NavigateToList(string listName)
    {
        _navigationService.OpenList(listName);
    }

    [RelayCommand]
    private void NavigateToReport(string reportName)
    {
        _navigationService.OpenReport(reportName);
    }
}
