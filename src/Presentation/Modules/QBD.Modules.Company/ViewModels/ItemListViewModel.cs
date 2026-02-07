using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Items;

namespace QBD.Modules.Company.ViewModels;

public partial class ItemListViewModel : ListViewModelBase<Item>
{
    private readonly IRepository<Item> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ItemListViewModel(IRepository<Item> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        Title = "Item List";
    }

    protected override async Task LoadItemsAsync()
    {
        IsBusy = true;
        try
        {
            var query = _repository.Query();
            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(i => i.ItemName.Contains(SearchText));

            var items = await query.OrderBy(i => i.ItemName).ToListAsync();
            Items = new System.Collections.ObjectModel.ObservableCollection<Item>(items);
            TotalRecords = items.Count;
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    public override async Task InitializeAsync() => await LoadItemsAsync();
}
