using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;

namespace QBD.Modules.Company.ViewModels;

public partial class ChartOfAccountsViewModel : ListViewModelBase<Account>
{
    private readonly IRepository<Account> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INavigationService _navigationService;

    public ChartOfAccountsViewModel(
        IRepository<Account> repository,
        IUnitOfWork unitOfWork,
        INavigationService navigationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _navigationService = navigationService;
        Title = "Chart of Accounts";
    }

    protected override async Task LoadItemsAsync()
    {
        IsBusy = true;
        try
        {
            var query = _repository.Query();
            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(a => a.Name.Contains(SearchText) || (a.Number != null && a.Number.Contains(SearchText)));

            var accounts = await query.OrderBy(a => a.SortOrder).ThenBy(a => a.Number).ToListAsync();
            Items = new System.Collections.ObjectModel.ObservableCollection<Account>(accounts);
            TotalRecords = accounts.Count;
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public override async Task InitializeAsync()
    {
        await LoadItemsAsync();
    }
}
