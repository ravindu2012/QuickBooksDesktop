using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Customers;
using QBD.Domain.Entities.Vendors;
using QBD.Domain.Entities.Items;
using QBD.Domain.Enums;
using QBD.Infrastructure.Data;
using QBD.Infrastructure.Services;

namespace QBD.UnitTests;

public class AccountingEngineTests
{
    private async Task<QBDesktopDbContext> GetInMemoryDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<QBDesktopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new QBDesktopDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    [Fact]
    public void JournalEntry_ShouldBeBalanced_WhenDebitsEqualsCredits()
    {
        // arrange: create a mock journal entry with equal debit and credit amounts
        var lines = new List<JournalEntryLine>
        {
            new JournalEntryLine { DebitAmount = 100.00m, CreditAmount = 0 },
            new JournalEntryLine { DebitAmount = 0, CreditAmount = 100.00m }
        };

        // act: calculate the difference between total debits and credits
        var totalDebits = lines.Sum(l => l.DebitAmount);
        var totalCredits = lines.Sum(l => l.CreditAmount);
        var balance = totalDebits - totalCredits;

        // assert: the balance should be exactly zero
        Assert.Equal(0, balance);
    }

    [Fact]
    public void JournalEntry_ShouldBeUnbalanced_WhenDebitsDoNotEqualCredits()
    {
        // arrange: create a mock journal entry with unequal amounts (100 vs 80)
        var lines = new List<JournalEntryLine>
        {
            new JournalEntryLine { DebitAmount = 100.00m, CreditAmount = 0 },
            new JournalEntryLine { DebitAmount = 0, CreditAmount = 80.00m }
        };

        // act: calculate the difference
        var totalDebits = lines.Sum(l => l.DebitAmount);
        var totalCredits = lines.Sum(l => l.CreditAmount);
        var balance = totalDebits - totalCredits;

        // assert: the balance should NOT be zero
        Assert.NotEqual(0, balance);
    }

    [Fact]
    public async Task ValidateBalanceAsync_ShouldReturnTrue_WhenBalanced()
    {
        // arrange: setup DB with balanced General Ledger (GL) entries
        var context = await GetInMemoryDbContextAsync();
        var service = new TransactionPostingService(context);

        context.GLEntries.AddRange(
            new GLEntry { DebitAmount = 100, CreditAmount = 0, TransactionType = TransactionType.JournalEntry },
            new GLEntry { DebitAmount = 0, CreditAmount = 100, TransactionType = TransactionType.JournalEntry }
        );
        await context.SaveChangesAsync();

        // act: execute the balance validation logic
        var result = await service.ValidateBalanceAsync();

        // assert: the service should confirm the ledger is balanced
        Assert.True(result);
    }

    [Fact]
    public async Task PostTransaction_ShouldThrowException_WhenUnbalanced()
    {
        // arrange: setup DB and create an unbalanced entry
        var context = await GetInMemoryDbContextAsync();
        var service = new TransactionPostingService(context);

        var je = new JournalEntry 
        { 
            Id = 1,
            EntryNumber = "JE-001",
            PostingDate = DateTime.Now,
            Lines = new List<JournalEntryLine> 
            {
                new JournalEntryLine { DebitAmount = 100, CreditAmount = 0 },
                new JournalEntryLine { DebitAmount = 0, CreditAmount = 50 }
            }
        };
        context.JournalEntries.Add(je);
        await context.SaveChangesAsync();

        // act & assert: attempting to post should throw an InvalidOperationException
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.PostTransactionAsync(TransactionType.JournalEntry, 1));
            
        Assert.Contains("Unbalanced entry", exception.Message);
    }

    [Fact]
    public async Task PostTransaction_ShouldCreateGLEntries_WhenBalanced()
    {
        // arrange: setup DB with required dummy accounts to prevent "not found" errors
        var context = await GetInMemoryDbContextAsync();
        var service = new TransactionPostingService(context);

        context.Accounts.Add(new Account { Id = 1, Name = "Bank", AccountType = AccountType.Bank, IsDebitNormal = true });
        context.Accounts.Add(new Account { Id = 2, Name = "Income", AccountType = AccountType.Income, IsDebitNormal = false });
        
        var je = new JournalEntry 
        { 
            Id = 2,
            EntryNumber = "JE-002",
            PostingDate = DateTime.Now,
            Lines = new List<JournalEntryLine> 
            {
                new JournalEntryLine { AccountId = 1, DebitAmount = 100, CreditAmount = 0 },
                new JournalEntryLine { AccountId = 2, DebitAmount = 0, CreditAmount = 100 } 
            }
        };
        context.JournalEntries.Add(je);
        await context.SaveChangesAsync();

        // act: post the valid transaction
        await service.PostTransactionAsync(TransactionType.JournalEntry, 2);

        // assert: verify that exactly 2 GL entries were saved to the database
        var entries = await context.GLEntries.Where(e => e.TransactionId == 2).ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Equal(100m, entries.Sum(e => e.DebitAmount));
        Assert.Equal(100m, entries.Sum(e => e.CreditAmount));
    }

    [Fact]
    public async Task VoidTransaction_ShouldReverseEntriesCorrectly()
    {
        // arrange: setup DB with an existing GL entry that needs to be voided
        var context = await GetInMemoryDbContextAsync();
        var service = new TransactionPostingService(context);

        context.Accounts.Add(new Account { Id = 1, Name = "Bank", AccountType = AccountType.Bank, IsDebitNormal = true, Balance = 100 });
        
        context.GLEntries.Add(new GLEntry 
        { 
            TransactionType = TransactionType.JournalEntry, 
            TransactionId = 99, 
            AccountId = 1, 
            DebitAmount = 100, 
            CreditAmount = 0,
            IsVoid = false
        });
        await context.SaveChangesAsync();

        // act: trigger the void process
        await service.VoidTransactionAsync(TransactionType.JournalEntry, 99);

        // assert: verify the original is marked void and a reversal entry was generated
        var allEntries = await context.GLEntries.Where(e => e.TransactionId == 99).ToListAsync();
        
        Assert.Equal(2, allEntries.Count); 
        Assert.True(allEntries.First(e => e.DebitAmount == 100).IsVoid);
        Assert.Contains(allEntries, e => e.CreditAmount == 100 && e.IsVoid); 
    }

    [Fact]
    public async Task PostInvoice_ShouldCreateCorrectGLEntries()
    {
        // arrange: setup Accounts, Customer, Item, and Invoice in the real DB
        var context = await GetInMemoryDbContextAsync();
        var service = new TransactionPostingService(context);

        // system accounts needed for invoices
        context.Accounts.Add(new Account { Id = 10, Name = "Accounts Receivable", AccountType = AccountType.AccountsReceivable, IsSystemAccount = true, IsDebitNormal = true });
        context.Accounts.Add(new Account { Id = 20, Name = "Sales Income", AccountType = AccountType.Income, IsSystemAccount = true, IsDebitNormal = false });
        
        var customer = new Customer { Id = 1, CustomerName = "Test Client" };
        var item = new Item { Id = 1, IncomeAccountId = 20 };
        context.Customers.Add(customer);
        context.Items.Add(item);

        var invoice = new Invoice
        {
            Id = 500,
            InvoiceNumber = "INV-001",
            Date = DateTime.Now,
            CustomerId = 1,
            Total = 150m,
            Lines = new List<InvoiceLine> { new InvoiceLine { ItemId = 1, Amount = 150m, Item = item } }
        };
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        // act: post the invoice
        await service.PostTransactionAsync(TransactionType.Invoice, 500);

        // assert: verify AR is debited and Income is credited
        var entries = await context.GLEntries.Where(e => e.TransactionId == 500 && e.TransactionType == TransactionType.Invoice).ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Contains(entries, e => e.AccountId == 10 && e.DebitAmount == 150m); 
        Assert.Contains(entries, e => e.AccountId == 20 && e.CreditAmount == 150m); 
    }

    [Fact]
    public async Task PostBillPayment_ShouldTriggerCorrectTransactionType()
    {
        // arrange: setup AP, Bank, Vendor, Bill, and BillPayment
        var context = await GetInMemoryDbContextAsync();
        var service = new TransactionPostingService(context);

        context.Accounts.Add(new Account { Id = 30, Name = "Accounts Payable", AccountType = AccountType.AccountsPayable, IsSystemAccount = true, IsDebitNormal = false });
        context.Accounts.Add(new Account { Id = 40, Name = "Checking", AccountType = AccountType.Bank, IsSystemAccount = true, IsDebitNormal = true });

        var vendor = new Vendor { Id = 1, VendorName = "Office Supplies Co." };
        var bill = new Bill { Id = 100, VendorId = 1, AmountDue = 200m, Vendor = vendor };
        context.Vendors.Add(vendor);
        context.Bills.Add(bill);

        var payment = new BillPayment
        {
            Id = 202,
            Date = DateTime.Now,
            PaymentAccountId = 40,
            Amount = 200m,
            Applications = new List<BillPaymentApplication> { new BillPaymentApplication { BillId = 100, AmountApplied = 200m, Bill = bill } }
        };
        context.BillPayments.Add(payment);
        await context.SaveChangesAsync();

        // act: post the bill payment
        await service.PostTransactionAsync(TransactionType.BillPayment, 202);

        // assert: verify AP is debited (reduced) and Bank is credited (reduced)
        var entries = await context.GLEntries.Where(e => e.TransactionId == 202 && e.TransactionType == TransactionType.BillPayment).ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Contains(entries, e => e.AccountId == 30 && e.DebitAmount == 200m);
        Assert.Contains(entries, e => e.AccountId == 40 && e.CreditAmount == 200m); 
    }
}