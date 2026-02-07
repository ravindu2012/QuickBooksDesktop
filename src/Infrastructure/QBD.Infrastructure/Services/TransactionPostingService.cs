using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Banking;
using QBD.Domain.Entities.Customers;
using QBD.Domain.Entities.Vendors;
using QBD.Domain.Enums;
using QBD.Infrastructure.Data;

namespace QBD.Infrastructure.Services;

public class TransactionPostingService : ITransactionPostingService
{
    private readonly QBDesktopDbContext _context;

    public TransactionPostingService(QBDesktopDbContext context)
    {
        _context = context;
    }

    public async Task PostTransactionAsync(TransactionType type, int transactionId)
    {
        var entries = type switch
        {
            TransactionType.Invoice => await CreateInvoiceEntriesAsync(transactionId),
            TransactionType.ReceivePayment => await CreateReceivePaymentEntriesAsync(transactionId),
            TransactionType.SalesReceipt => await CreateSalesReceiptEntriesAsync(transactionId),
            TransactionType.CreditMemo => await CreateCreditMemoEntriesAsync(transactionId),
            TransactionType.Bill => await CreateBillEntriesAsync(transactionId),
            TransactionType.BillPayment => await CreateBillPaymentEntriesAsync(transactionId),
            TransactionType.VendorCredit => await CreateVendorCreditEntriesAsync(transactionId),
            TransactionType.Check => await CreateCheckEntriesAsync(transactionId),
            TransactionType.Deposit => await CreateDepositEntriesAsync(transactionId),
            TransactionType.Transfer => await CreateTransferEntriesAsync(transactionId),
            TransactionType.JournalEntry => await CreateJournalEntryEntriesAsync(transactionId),
            _ => throw new ArgumentException($"Unsupported transaction type: {type}")
        };

        // Validate balanced
        var totalDebits = entries.Sum(e => e.DebitAmount);
        var totalCredits = entries.Sum(e => e.CreditAmount);
        if (totalDebits != totalCredits)
            throw new InvalidOperationException($"Unbalanced entry: Debits={totalDebits}, Credits={totalCredits}");

        await _context.GLEntries.AddRangeAsync(entries);

        // Update account balances
        foreach (var entry in entries)
        {
            var account = await _context.Accounts.FindAsync(entry.AccountId)
                ?? throw new InvalidOperationException($"Account {entry.AccountId} not found");
            if (account.IsDebitNormal)
                account.Balance += entry.DebitAmount - entry.CreditAmount;
            else
                account.Balance += entry.CreditAmount - entry.DebitAmount;
        }

        await _context.SaveChangesAsync();
    }

    public async Task VoidTransactionAsync(TransactionType type, int transactionId)
    {
        var originalEntries = await _context.GLEntries
            .Where(e => e.TransactionType == type && e.TransactionId == transactionId && !e.IsVoid)
            .ToListAsync();

        foreach (var original in originalEntries)
        {
            original.IsVoid = true;

            var reversal = new GLEntry
            {
                PostingDate = DateTime.Today,
                AccountId = original.AccountId,
                DebitAmount = original.CreditAmount,
                CreditAmount = original.DebitAmount,
                TransactionType = type,
                TransactionId = transactionId,
                TransactionNumber = original.TransactionNumber,
                Memo = $"VOID: {original.Memo}",
                NameType = original.NameType,
                NameId = original.NameId,
                NameDisplay = original.NameDisplay,
                ClassId = original.ClassId,
                LocationId = original.LocationId,
                FiscalPeriodId = original.FiscalPeriodId,
                IsVoid = true
            };
            await _context.GLEntries.AddAsync(reversal);

            // Reverse account balance
            var account = await _context.Accounts.FindAsync(original.AccountId);
            if (account != null)
            {
                if (account.IsDebitNormal)
                    account.Balance += reversal.DebitAmount - reversal.CreditAmount;
                else
                    account.Balance += reversal.CreditAmount - reversal.DebitAmount;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateBalanceAsync()
    {
        var totalDebits = await _context.GLEntries.SumAsync(e => e.DebitAmount);
        var totalCredits = await _context.GLEntries.SumAsync(e => e.CreditAmount);
        return totalDebits == totalCredits;
    }

    private async Task<List<GLEntry>> CreateInvoiceEntriesAsync(int invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines).ThenInclude(l => l.Item)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Id == invoiceId)
            ?? throw new InvalidOperationException($"Invoice {invoiceId} not found");

        var arAccount = await GetSystemAccountAsync(AccountType.AccountsReceivable);
        var entries = new List<GLEntry>();

        // Debit AR for the full amount
        entries.Add(new GLEntry
        {
            PostingDate = invoice.Date,
            AccountId = arAccount.Id,
            DebitAmount = invoice.Total,
            CreditAmount = 0,
            TransactionType = TransactionType.Invoice,
            TransactionId = invoiceId,
            TransactionNumber = invoice.InvoiceNumber,
            Memo = $"Invoice {invoice.InvoiceNumber}",
            NameType = "Customer",
            NameId = invoice.CustomerId,
            NameDisplay = invoice.Customer.CustomerName
        });

        // Credit Income for each line
        foreach (var line in invoice.Lines)
        {
            var incomeAccountId = line.Item?.IncomeAccountId
                ?? (await GetDefaultIncomeAccountAsync()).Id;
            entries.Add(new GLEntry
            {
                PostingDate = invoice.Date,
                AccountId = incomeAccountId,
                DebitAmount = 0,
                CreditAmount = line.Amount,
                TransactionType = TransactionType.Invoice,
                TransactionId = invoiceId,
                TransactionNumber = invoice.InvoiceNumber,
                Memo = line.Description ?? $"Invoice {invoice.InvoiceNumber} line",
                NameType = "Customer",
                NameId = invoice.CustomerId,
                NameDisplay = invoice.Customer.CustomerName,
                ClassId = line.ClassId
            });
        }

        // Credit Sales Tax Payable if tax exists
        if (invoice.TaxTotal > 0)
        {
            var taxAccount = await GetOrCreateAccountAsync("Sales Tax Payable", AccountType.OtherCurrentLiability);
            entries.Add(new GLEntry
            {
                PostingDate = invoice.Date,
                AccountId = taxAccount.Id,
                DebitAmount = 0,
                CreditAmount = invoice.TaxTotal,
                TransactionType = TransactionType.Invoice,
                TransactionId = invoiceId,
                TransactionNumber = invoice.InvoiceNumber,
                Memo = $"Tax on Invoice {invoice.InvoiceNumber}"
            });
        }

        invoice.Status = DocStatus.Posted;
        return entries;
    }

    private async Task<List<GLEntry>> CreateReceivePaymentEntriesAsync(int paymentId)
    {
        var payment = await _context.ReceivePayments
            .Include(p => p.Customer)
            .Include(p => p.Applications)
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new InvalidOperationException($"Payment {paymentId} not found");

        var entries = new List<GLEntry>();
        var depositAccountId = payment.DepositToAccountId
            ?? (await GetOrCreateAccountAsync("Undeposited Funds", AccountType.OtherCurrentAsset)).Id;

        // Debit Bank/Undeposited Funds
        entries.Add(new GLEntry
        {
            PostingDate = payment.PaymentDate,
            AccountId = depositAccountId,
            DebitAmount = payment.Amount,
            CreditAmount = 0,
            TransactionType = TransactionType.ReceivePayment,
            TransactionId = paymentId,
            Memo = $"Payment from {payment.Customer.CustomerName}",
            NameType = "Customer",
            NameId = payment.CustomerId,
            NameDisplay = payment.Customer.CustomerName
        });

        // Credit AR
        var arAccount = await GetSystemAccountAsync(AccountType.AccountsReceivable);
        entries.Add(new GLEntry
        {
            PostingDate = payment.PaymentDate,
            AccountId = arAccount.Id,
            DebitAmount = 0,
            CreditAmount = payment.Amount,
            TransactionType = TransactionType.ReceivePayment,
            TransactionId = paymentId,
            Memo = $"Payment from {payment.Customer.CustomerName}",
            NameType = "Customer",
            NameId = payment.CustomerId,
            NameDisplay = payment.Customer.CustomerName
        });

        // Update invoice balances
        foreach (var app in payment.Applications)
        {
            var invoice = await _context.Invoices.FindAsync(app.InvoiceId);
            if (invoice != null)
            {
                invoice.AmountPaid += app.AmountApplied;
                invoice.BalanceDue -= app.AmountApplied;
            }
        }

        // Update customer balance
        payment.Customer.Balance -= payment.Amount;
        payment.Status = DocStatus.Posted;
        return entries;
    }

    private async Task<List<GLEntry>> CreateSalesReceiptEntriesAsync(int receiptId)
    {
        var receipt = await _context.SalesReceipts
            .Include(sr => sr.Lines).ThenInclude(l => l.Item)
            .Include(sr => sr.Customer)
            .FirstOrDefaultAsync(sr => sr.Id == receiptId)
            ?? throw new InvalidOperationException($"Sales Receipt {receiptId} not found");

        var entries = new List<GLEntry>();
        var depositAccountId = receipt.DepositToAccountId
            ?? (await GetOrCreateAccountAsync("Undeposited Funds", AccountType.OtherCurrentAsset)).Id;

        // Debit Bank/Undeposited Funds
        entries.Add(new GLEntry
        {
            PostingDate = receipt.Date,
            AccountId = depositAccountId,
            DebitAmount = receipt.Total,
            CreditAmount = 0,
            TransactionType = TransactionType.SalesReceipt,
            TransactionId = receiptId,
            Memo = $"Sales Receipt",
            NameType = receipt.CustomerId != null ? "Customer" : null,
            NameId = receipt.CustomerId,
            NameDisplay = receipt.Customer?.CustomerName
        });

        // Credit Income per line
        foreach (var line in receipt.Lines)
        {
            var incomeAccountId = line.Item?.IncomeAccountId
                ?? (await GetDefaultIncomeAccountAsync()).Id;
            entries.Add(new GLEntry
            {
                PostingDate = receipt.Date,
                AccountId = incomeAccountId,
                DebitAmount = 0,
                CreditAmount = line.Amount,
                TransactionType = TransactionType.SalesReceipt,
                TransactionId = receiptId,
                Memo = line.Description,
                ClassId = line.ClassId
            });
        }

        if (receipt.Tax > 0)
        {
            var taxAccount = await GetOrCreateAccountAsync("Sales Tax Payable", AccountType.OtherCurrentLiability);
            entries.Add(new GLEntry
            {
                PostingDate = receipt.Date,
                AccountId = taxAccount.Id,
                DebitAmount = 0,
                CreditAmount = receipt.Tax,
                TransactionType = TransactionType.SalesReceipt,
                TransactionId = receiptId,
                Memo = "Sales Tax"
            });
        }

        receipt.Status = DocStatus.Posted;
        return entries;
    }

    private async Task<List<GLEntry>> CreateCreditMemoEntriesAsync(int creditMemoId)
    {
        var cm = await _context.CreditMemos
            .Include(c => c.Lines).ThenInclude(l => l.Item)
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == creditMemoId)
            ?? throw new InvalidOperationException($"Credit Memo {creditMemoId} not found");

        var arAccount = await GetSystemAccountAsync(AccountType.AccountsReceivable);
        var entries = new List<GLEntry>();

        // Credit AR (reduce receivable)
        entries.Add(new GLEntry
        {
            PostingDate = cm.Date,
            AccountId = arAccount.Id,
            DebitAmount = 0,
            CreditAmount = cm.Total,
            TransactionType = TransactionType.CreditMemo,
            TransactionId = creditMemoId,
            TransactionNumber = cm.CreditNumber,
            Memo = $"Credit Memo {cm.CreditNumber}",
            NameType = "Customer",
            NameId = cm.CustomerId,
            NameDisplay = cm.Customer.CustomerName
        });

        // Debit Income per line (reverse revenue)
        foreach (var line in cm.Lines)
        {
            var incomeAccountId = line.Item?.IncomeAccountId
                ?? (await GetDefaultIncomeAccountAsync()).Id;
            entries.Add(new GLEntry
            {
                PostingDate = cm.Date,
                AccountId = incomeAccountId,
                DebitAmount = line.Amount,
                CreditAmount = 0,
                TransactionType = TransactionType.CreditMemo,
                TransactionId = creditMemoId,
                TransactionNumber = cm.CreditNumber,
                Memo = line.Description,
                ClassId = line.ClassId
            });
        }

        cm.Customer.Balance -= cm.Total;
        cm.Status = DocStatus.Posted;
        return entries;
    }

    private async Task<List<GLEntry>> CreateBillEntriesAsync(int billId)
    {
        var bill = await _context.Bills
            .Include(b => b.ExpenseLines)
            .Include(b => b.ItemLines).ThenInclude(l => l.Item)
            .Include(b => b.Vendor)
            .FirstOrDefaultAsync(b => b.Id == billId)
            ?? throw new InvalidOperationException($"Bill {billId} not found");

        var apAccount = await GetSystemAccountAsync(AccountType.AccountsPayable);
        var entries = new List<GLEntry>();

        // Debit Expense/COGS per expense line
        foreach (var line in bill.ExpenseLines)
        {
            entries.Add(new GLEntry
            {
                PostingDate = bill.Date,
                AccountId = line.AccountId,
                DebitAmount = line.Amount,
                CreditAmount = 0,
                TransactionType = TransactionType.Bill,
                TransactionId = billId,
                Memo = line.Memo ?? $"Bill from {bill.Vendor.VendorName}",
                NameType = "Vendor",
                NameId = bill.VendorId,
                NameDisplay = bill.Vendor.VendorName,
                ClassId = line.ClassId
            });
        }

        // Debit Expense/COGS per item line
        foreach (var line in bill.ItemLines)
        {
            var expenseAccountId = line.Item?.ExpenseAccountId
                ?? (await GetDefaultExpenseAccountAsync()).Id;
            entries.Add(new GLEntry
            {
                PostingDate = bill.Date,
                AccountId = expenseAccountId,
                DebitAmount = line.Amount,
                CreditAmount = 0,
                TransactionType = TransactionType.Bill,
                TransactionId = billId,
                Memo = line.Description ?? $"Bill from {bill.Vendor.VendorName}",
                NameType = "Vendor",
                NameId = bill.VendorId,
                NameDisplay = bill.Vendor.VendorName,
                ClassId = line.ClassId
            });
        }

        // Credit AP
        entries.Add(new GLEntry
        {
            PostingDate = bill.Date,
            AccountId = apAccount.Id,
            DebitAmount = 0,
            CreditAmount = bill.AmountDue,
            TransactionType = TransactionType.Bill,
            TransactionId = billId,
            Memo = $"Bill from {bill.Vendor.VendorName}",
            NameType = "Vendor",
            NameId = bill.VendorId,
            NameDisplay = bill.Vendor.VendorName
        });

        bill.Vendor.Balance += bill.AmountDue;
        bill.Status = DocStatus.Posted;
        return entries;
    }

    private async Task<List<GLEntry>> CreateBillPaymentEntriesAsync(int paymentId)
    {
        var payment = await _context.BillPayments
            .Include(bp => bp.Applications).ThenInclude(a => a.Bill).ThenInclude(b => b.Vendor)
            .Include(bp => bp.PaymentAccount)
            .FirstOrDefaultAsync(bp => bp.Id == paymentId)
            ?? throw new InvalidOperationException($"Bill Payment {paymentId} not found");

        var apAccount = await GetSystemAccountAsync(AccountType.AccountsPayable);
        var entries = new List<GLEntry>();

        // Debit AP
        entries.Add(new GLEntry
        {
            PostingDate = payment.Date,
            AccountId = apAccount.Id,
            DebitAmount = payment.Amount,
            CreditAmount = 0,
            TransactionType = TransactionType.BillPayment,
            TransactionId = paymentId,
            Memo = "Bill Payment"
        });

        // Credit Bank
        entries.Add(new GLEntry
        {
            PostingDate = payment.Date,
            AccountId = payment.PaymentAccountId,
            DebitAmount = 0,
            CreditAmount = payment.Amount,
            TransactionType = TransactionType.BillPayment,
            TransactionId = paymentId,
            Memo = "Bill Payment"
        });

        // Update bill balances
        foreach (var app in payment.Applications)
        {
            app.Bill.AmountPaid += app.AmountApplied;
            app.Bill.BalanceDue -= app.AmountApplied;
            app.Bill.Vendor.Balance -= app.AmountApplied;
        }

        payment.Status = DocStatus.Posted;
        return entries;
    }

    private async Task<List<GLEntry>> CreateVendorCreditEntriesAsync(int creditId)
    {
        var credit = await _context.VendorCredits
            .Include(vc => vc.Lines)
            .Include(vc => vc.Vendor)
            .FirstOrDefaultAsync(vc => vc.Id == creditId)
            ?? throw new InvalidOperationException($"Vendor Credit {creditId} not found");

        var apAccount = await GetSystemAccountAsync(AccountType.AccountsPayable);
        var entries = new List<GLEntry>();

        // Debit AP (reduce payable)
        entries.Add(new GLEntry
        {
            PostingDate = credit.Date,
            AccountId = apAccount.Id,
            DebitAmount = credit.Total,
            CreditAmount = 0,
            TransactionType = TransactionType.VendorCredit,
            TransactionId = creditId,
            Memo = $"Vendor Credit from {credit.Vendor.VendorName}",
            NameType = "Vendor",
            NameId = credit.VendorId,
            NameDisplay = credit.Vendor.VendorName
        });

        // Credit Expense per line (reverse expense)
        foreach (var line in credit.Lines)
        {
            var accountId = line.AccountId ?? (await GetDefaultExpenseAccountAsync()).Id;
            entries.Add(new GLEntry
            {
                PostingDate = credit.Date,
                AccountId = accountId,
                DebitAmount = 0,
                CreditAmount = line.Amount,
                TransactionType = TransactionType.VendorCredit,
                TransactionId = creditId,
                Memo = line.Memo,
                ClassId = line.ClassId
            });
        }

        credit.Vendor.Balance -= credit.Total;
        credit.Status = DocStatus.Posted;
        return entries;
    }

    private async Task<List<GLEntry>> CreateCheckEntriesAsync(int checkId)
    {
        var check = await _context.Checks
            .Include(c => c.ExpenseLines)
            .Include(c => c.ItemLines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(c => c.Id == checkId)
            ?? throw new InvalidOperationException($"Check {checkId} not found");

        var entries = new List<GLEntry>();

        // Debit Expense per line
        foreach (var line in check.ExpenseLines)
        {
            entries.Add(new GLEntry
            {
                PostingDate = check.Date,
                AccountId = line.AccountId,
                DebitAmount = line.Amount,
                CreditAmount = 0,
                TransactionType = TransactionType.Check,
                TransactionId = checkId,
                TransactionNumber = check.CheckNumber,
                Memo = line.Memo ?? check.Memo,
                ClassId = line.ClassId
            });
        }

        foreach (var line in check.ItemLines)
        {
            var expenseAccountId = line.Item?.ExpenseAccountId
                ?? (await GetDefaultExpenseAccountAsync()).Id;
            entries.Add(new GLEntry
            {
                PostingDate = check.Date,
                AccountId = expenseAccountId,
                DebitAmount = line.Amount,
                CreditAmount = 0,
                TransactionType = TransactionType.Check,
                TransactionId = checkId,
                TransactionNumber = check.CheckNumber,
                Memo = line.Description ?? check.Memo
            });
        }

        // Credit Bank
        entries.Add(new GLEntry
        {
            PostingDate = check.Date,
            AccountId = check.BankAccountId,
            DebitAmount = 0,
            CreditAmount = check.Amount,
            TransactionType = TransactionType.Check,
            TransactionId = checkId,
            TransactionNumber = check.CheckNumber,
            Memo = check.Memo
        });

        check.Status = DocStatus.Posted;
        return entries;
    }

    private async Task<List<GLEntry>> CreateDepositEntriesAsync(int depositId)
    {
        var deposit = await _context.Deposits
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == depositId)
            ?? throw new InvalidOperationException($"Deposit {depositId} not found");

        var entries = new List<GLEntry>();

        // Debit Bank
        entries.Add(new GLEntry
        {
            PostingDate = deposit.Date,
            AccountId = deposit.BankAccountId,
            DebitAmount = deposit.Total,
            CreditAmount = 0,
            TransactionType = TransactionType.Deposit,
            TransactionId = depositId,
            Memo = deposit.Memo ?? "Deposit"
        });

        // Credit Undeposited Funds / other per line
        foreach (var line in deposit.Lines)
        {
            var fromAccountId = line.FromAccountId
                ?? (await GetOrCreateAccountAsync("Undeposited Funds", AccountType.OtherCurrentAsset)).Id;
            entries.Add(new GLEntry
            {
                PostingDate = deposit.Date,
                AccountId = fromAccountId,
                DebitAmount = 0,
                CreditAmount = line.Amount,
                TransactionType = TransactionType.Deposit,
                TransactionId = depositId,
                Memo = line.Memo ?? $"From {line.ReceivedFrom}"
            });
        }

        deposit.Status = DocStatus.Posted;
        return entries;
    }

    private async Task<List<GLEntry>> CreateTransferEntriesAsync(int transferId)
    {
        var transfer = await _context.Transfers
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .FirstOrDefaultAsync(t => t.Id == transferId)
            ?? throw new InvalidOperationException($"Transfer {transferId} not found");

        var entries = new List<GLEntry>
        {
            // Debit To-Account
            new GLEntry
            {
                PostingDate = transfer.Date,
                AccountId = transfer.ToAccountId,
                DebitAmount = transfer.Amount,
                CreditAmount = 0,
                TransactionType = TransactionType.Transfer,
                TransactionId = transferId,
                Memo = $"Transfer from {transfer.FromAccount.Name}"
            },
            // Credit From-Account
            new GLEntry
            {
                PostingDate = transfer.Date,
                AccountId = transfer.FromAccountId,
                DebitAmount = 0,
                CreditAmount = transfer.Amount,
                TransactionType = TransactionType.Transfer,
                TransactionId = transferId,
                Memo = $"Transfer to {transfer.ToAccount.Name}"
            }
        };

        transfer.Status = DocStatus.Posted;
        return entries;
    }

    private async Task<List<GLEntry>> CreateJournalEntryEntriesAsync(int journalEntryId)
    {
        var je = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == journalEntryId)
            ?? throw new InvalidOperationException($"Journal Entry {journalEntryId} not found");

        var entries = je.Lines.Select(line => new GLEntry
        {
            PostingDate = je.PostingDate,
            AccountId = line.AccountId,
            DebitAmount = line.DebitAmount,
            CreditAmount = line.CreditAmount,
            TransactionType = TransactionType.JournalEntry,
            TransactionId = journalEntryId,
            TransactionNumber = je.EntryNumber,
            Memo = line.Memo ?? je.Memo,
            NameId = line.NameId,
            ClassId = line.ClassId
        }).ToList();

        je.Status = DocStatus.Posted;
        return entries;
    }

    private async Task<Account> GetSystemAccountAsync(AccountType type)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.AccountType == type && !a.IsSubAccount)
            ?? throw new InvalidOperationException($"System account of type {type} not found. Please set up Chart of Accounts.");
    }

    private async Task<Account> GetDefaultIncomeAccountAsync()
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.AccountType == AccountType.Income && !a.IsSubAccount)
            ?? throw new InvalidOperationException("No income account found.");
    }

    private async Task<Account> GetDefaultExpenseAccountAsync()
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.AccountType == AccountType.Expense && !a.IsSubAccount)
            ?? throw new InvalidOperationException("No expense account found.");
    }

    private async Task<Account> GetOrCreateAccountAsync(string name, AccountType type)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Name == name && a.AccountType == type);
        if (account != null) return account;

        account = new Account
        {
            Name = name,
            AccountType = type,
            IsActive = true,
            IsSystemAccount = true,
            IsDebitNormal = type == AccountType.Bank || type == AccountType.AccountsReceivable ||
                           type == AccountType.OtherCurrentAsset || type == AccountType.FixedAsset ||
                           type == AccountType.OtherAsset || type == AccountType.Expense ||
                           type == AccountType.OtherExpense || type == AccountType.CostOfGoodsSold
        };
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();
        return account;
    }
}
