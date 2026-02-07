using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;

namespace QBD.Modules.Company.ViewModels;

public partial class TermsListViewModel : ListViewModelBase<Terms>
{
    private readonly IRepository<Terms> _repository;

    public TermsListViewModel(IRepository<Terms> repository)
    {
        _repository = repository;
        Title = "Terms List";
    }

    protected override async Task LoadItemsAsync()
    {
        var terms = await _repository.Query().OrderBy(t => t.DueDays).ToListAsync();
        Items = new System.Collections.ObjectModel.ObservableCollection<Terms>(terms);
        TotalRecords = terms.Count;
    }

    public override async Task InitializeAsync() => await LoadItemsAsync();
}
