using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Company;

namespace QBD.Modules.Company.ViewModels;

public partial class CompanyInfoFormViewModel : ViewModelBase
{
    private readonly IRepository<CompanyInfo> _repository;
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty] private string _companyName = string.Empty;
    [ObservableProperty] private string? _legalName;
    [ObservableProperty] private string? _address;
    [ObservableProperty] private string? _city;
    [ObservableProperty] private string? _state;
    [ObservableProperty] private string? _zip;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _ein;
    [ObservableProperty] private int _fiscalYearStartMonth = 1;

    private int? _companyId;

    public CompanyInfoFormViewModel(IRepository<CompanyInfo> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        Title = "Company Information";
    }

    public override async Task InitializeAsync()
    {
        var companies = await _repository.GetAllAsync();
        var company = companies.FirstOrDefault();
        if (company != null)
        {
            _companyId = company.Id;
            CompanyName = company.Name;
            LegalName = company.LegalName;
            Address = company.Address;
            City = company.City;
            State = company.State;
            Zip = company.Zip;
            Phone = company.Phone;
            Email = company.Email;
            Ein = company.EIN;
            FiscalYearStartMonth = company.FiscalYearStartMonth;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            CompanyInfo company;
            if (_companyId.HasValue)
            {
                company = (await _repository.GetByIdAsync(_companyId.Value))!;
            }
            else
            {
                company = new CompanyInfo();
                await _repository.AddAsync(company);
            }

            company.Name = CompanyName;
            company.LegalName = LegalName;
            company.Address = Address;
            company.City = City;
            company.State = State;
            company.Zip = Zip;
            company.Phone = Phone;
            company.Email = Email;
            company.EIN = Ein;
            company.FiscalYearStartMonth = FiscalYearStartMonth;

            if (_companyId.HasValue)
                await _repository.UpdateAsync(company);

            await _unitOfWork.SaveChangesAsync();
            _companyId = company.Id;
            SetStatus("Company information saved.");
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
