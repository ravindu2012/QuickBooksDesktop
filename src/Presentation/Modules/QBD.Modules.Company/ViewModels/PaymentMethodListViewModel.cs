using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;

namespace QBD.Modules.Company.ViewModels;

public partial class PaymentMethodListViewModel : ListViewModelBase<PaymentMethod>
{
    private readonly IRepository<PaymentMethod> _repository;

    public PaymentMethodListViewModel(IRepository<PaymentMethod> repository)
    {
        _repository = repository;
        Title = "Payment Method List";
    }

    protected override async Task LoadItemsAsync()
    {
        var methods = await _repository.Query().OrderBy(m => m.Name).ToListAsync();
        Items = new System.Collections.ObjectModel.ObservableCollection<PaymentMethod>(methods);
        TotalRecords = methods.Count;
    }

    public override async Task InitializeAsync() => await LoadItemsAsync();
}
