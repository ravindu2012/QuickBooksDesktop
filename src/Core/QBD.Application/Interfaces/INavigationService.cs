namespace QBD.Application.Interfaces;

public interface INavigationService
{
    void OpenTab(object viewModel);
    void CloseTab(object viewModel);
    void OpenHomePage();
    void OpenCenter(string centerName);
    void OpenForm(string formName, int? entityId = null);
    void OpenRegister(int accountId);
    void OpenReport(string reportName);
    void OpenList(string listName);
}
