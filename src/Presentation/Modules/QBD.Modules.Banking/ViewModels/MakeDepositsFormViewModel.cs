using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Banking;
using QBD.Domain.Enums;

namespace QBD.Modules.Banking.ViewModels;

public partial class MakeDepositsFormViewModel : TransactionFormViewModelBase<Deposit, DepositLine>
{
    private readonly IRepository<Deposit> _depositRepository;
    private readonly IRepository<Account> _accountRepository;

    [ObservableProperty] private ObservableCollection<Account> _bankAccounts = new();

    public MakeDepositsFormViewModel(
        IUnitOfWork unitOfWork, ITransactionPostingService postingService,
        INavigationService navigationService, IRepository<Deposit> depositRepository,
        IRepository<Account> accountRepository) : base(unitOfWork, postingService, navigationService)
    {
        _depositRepository = depositRepository; _accountRepository = accountRepository;
        Title = "Make Deposits";
    }

    public override async Task InitializeAsync()
    {
        BankAccounts = new ObservableCollection<Account>(await _accountRepository.Query().Where(a => a.AccountType == AccountType.Bank).OrderBy(a => a.Name).ToListAsync());
        Header = new Deposit { Date = DateTime.Today, Status = DocStatus.Draft };
        Lines.Add(new DepositLine());
    }

    protected override void RecalculateTotals()
    {
        GrandTotal = Lines.Sum(l => l.Amount);
        Header.Total = GrandTotal;
    }

    protected override async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            Header.Lines = Lines.Where(l => l.Amount != 0).ToList();
            RecalculateTotals();
            if (Header.Id == 0) await _depositRepository.AddAsync(Header);
            else await _depositRepository.UpdateAsync(Header);
            await UnitOfWork.SaveChangesAsync();
            SetStatus("Deposit saved.");
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    protected override async Task SaveAndPostAsync()
    {
        await SaveAsync();
        if (ErrorMessage != null) return;
        try { await PostingService.PostTransactionAsync(TransactionType.Deposit, Header.Id); Status = DocStatus.Posted; IsEditable = false; SetStatus("Deposit posted."); }
        catch (Exception ex) { SetError(ex.Message); }
    }

    protected override async Task VoidAsync()
    {
        if (Header.Id == 0) return;
        try { await PostingService.VoidTransactionAsync(TransactionType.Deposit, Header.Id); Status = DocStatus.Voided; IsEditable = false; }
        catch (Exception ex) { SetError(ex.Message); }
    }
}
