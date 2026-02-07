using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;

namespace QBD.Modules.Company.ViewModels;

public partial class ClassListViewModel : ListViewModelBase<Class>
{
    private readonly IRepository<Class> _repository;

    public ClassListViewModel(IRepository<Class> repository)
    {
        _repository = repository;
        Title = "Class List";
    }

    protected override async Task LoadItemsAsync()
    {
        var classes = await _repository.Query().OrderBy(c => c.Name).ToListAsync();
        Items = new System.Collections.ObjectModel.ObservableCollection<Class>(classes);
        TotalRecords = classes.Count;
    }

    public override async Task InitializeAsync() => await LoadItemsAsync();
}
