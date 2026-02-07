using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Company;

namespace QBD.Modules.Company.ViewModels;

public partial class PreferencesFormViewModel : ViewModelBase
{
    private readonly IRepository<Preference> _repository;
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty]
    private ObservableCollection<Preference> _preferences = new();

    public PreferencesFormViewModel(IRepository<Preference> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        Title = "Preferences";
    }

    public override async Task InitializeAsync()
    {
        var prefs = await _repository.GetAllAsync();
        Preferences = new ObservableCollection<Preference>(prefs);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            foreach (var pref in Preferences)
            {
                await _repository.UpdateAsync(pref);
            }
            await _unitOfWork.SaveChangesAsync();
            SetStatus("Preferences saved.");
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
}
