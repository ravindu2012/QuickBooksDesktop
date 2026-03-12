// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

using QBD.Domain.Enums;

namespace QBD.Application.Interfaces;

public interface ITransactionPostingService
{
    Task PostTransactionAsync(TransactionType type, int transactionId);
    Task VoidTransactionAsync(TransactionType type, int transactionId);
    Task<bool> ValidateBalanceAsync();
}
