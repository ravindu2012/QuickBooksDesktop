using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Banking;
using QBD.Domain.Enums;

namespace QBD.Modules.Banking.ViewModels;

public partial class TransferFundsFormViewModel : ViewModelBase
{
    private readonly IRepository<Transfer> _transferRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionPostingService _postingService;

    [ObservableProperty] private ObservableCollection<Account> _accounts = new();
    [ObservableProperty] private Account? _fromAccount;
    [ObservableProperty] private Account? _toAccount;
    [ObservableProperty] private decimal _amount;
    [ObservableProperty] private DateTime _date = DateTime.Today;
    [ObservableProperty] private string? _memo;

    private Transfer? _currentTransfer;

    public TransferFundsFormViewModel(
        IRepository<Transfer> transferRepository,
        IRepository<Account> accountRepository,
        IUnitOfWork unitOfWork,
        ITransactionPostingService postingService)
    {
        _transferRepository = transferRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _postingService = postingService;
        Title = "Transfer Funds";
    }

    public override async Task InitializeAsync()
    {
        Accounts = new ObservableCollection<Account>(await _accountRepository.Query().Where(a => a.AccountType == AccountType.Bank).OrderBy(a => a.Name).ToListAsync());
    }

    [RelayCommand]
    private async Task SaveAndPostAsync()
    {
        IsBusy = true;
        try
        {
            _currentTransfer = new Transfer
            {
                FromAccountId = FromAccount!.Id,
                ToAccountId = ToAccount!.Id,
                Date = Date,
                Amount = Amount,
                Memo = Memo,
                Status = DocStatus.Draft
            };
            await _transferRepository.AddAsync(_currentTransfer);
            await _unitOfWork.SaveChangesAsync();
            await _postingService.PostTransactionAsync(TransactionType.Transfer, _currentTransfer.Id);
            SetStatus("Transfer posted.");
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
