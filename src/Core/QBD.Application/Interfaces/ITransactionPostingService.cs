using QBD.Domain.Enums;

namespace QBD.Application.Interfaces;

public interface ITransactionPostingService
{
    Task PostTransactionAsync(TransactionType type, int transactionId);
    Task VoidTransactionAsync(TransactionType type, int transactionId);
    Task<bool> ValidateBalanceAsync();
}
