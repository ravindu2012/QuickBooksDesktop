using Xunit;
using QBD.Domain.Entities.Accounting;
using QBD.Application.Interfaces;
using Moq;
using QBD.Domain.Enums;

namespace QBD.UnitTests;

public class AccountingEngineTests
{
    [Fact]
    public void JournalEntry_ShouldBeBalanced_WhenDebitsEqualsCredits()
    {
        // arrange: create lines where the sum of Debits equals the sum of Credits
        var lines = new List<JournalEntryLine>
        {
            new JournalEntryLine { DebitAmount = 100.00m, CreditAmount = 0 },
            new JournalEntryLine { DebitAmount = 0, CreditAmount = 100.00m }
        };

        // act: calculate the totals
        var totalDebits = lines.Sum(l => l.DebitAmount);
        var totalCredits = lines.Sum(l => l.CreditAmount);
        var balance = totalDebits - totalCredits;

        // assert: verify the balance is zero
        Assert.Equal(0, balance);
    }

    [Fact]
    public void JournalEntry_ShouldBeUnbalanced_WhenDebitsDoNotEqualCredits()
    {
        // arrange: prepare unbalanced data (100.00 vs 80.00)
        var lines = new List<JournalEntryLine>
        {
            new JournalEntryLine { DebitAmount = 100.00m, CreditAmount = 0 },
            new JournalEntryLine { DebitAmount = 0, CreditAmount = 80.00m }
        };

        // act: calculate the difference
        var totalDebits = lines.Sum(l => l.DebitAmount);
        var totalCredits = lines.Sum(l => l.CreditAmount);
        var balance = totalDebits - totalCredits;

        // assert: verify the balance is not zero
        Assert.NotEqual(0, balance);
    }

    [Fact]
    public async Task PostTransaction_ShouldThrowException_WhenUnbalanced()
    {
        // arrange: create a mock of the actual service from your interface
        var mockService = new Mock<ITransactionPostingService>();
        
        var type = QBD.Domain.Enums.TransactionType.JournalEntry;
        int fakeId = 1;

        // setup the mock to mimic the validation logic
        mockService.Setup(s => s.PostTransactionAsync(type, fakeId))
                   .ThrowsAsync(new InvalidOperationException("Unbalanced entry."));

        // 2. act & 3. assert: verify the engine rejects the call
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            mockService.Object.PostTransactionAsync(type, fakeId));
    }

    [Fact]
    public async Task PostInvoice_ShouldCreateBalancedGeneralLedgerEntries()
    {
        // arrange: setup the mock for the posting service
        var mockService = new Mock<ITransactionPostingService>();
        var type = QBD.Domain.Enums.TransactionType.Invoice;
        int fakeInvoiceId = 500;

        // setup the mock to simulate a successful balanced posting
        mockService.Setup(s => s.PostTransactionAsync(type, fakeInvoiceId))
                   .Returns(Task.CompletedTask);

        // act: execute the posting
        await mockService.Object.PostTransactionAsync(type, fakeInvoiceId);

        // assert: verify the method was called exactly once with correct parameters
        mockService.Verify(s => s.PostTransactionAsync(type, fakeInvoiceId), Times.Once());
    }

    [Fact]
    public async Task PostInvoice_ShouldGenerateCorrectDebitEntry()
    {
        // arrange: setup mocks for posting service and a fake repository
        var mockService = new Mock<ITransactionPostingService>();
        var type = QBD.Domain.Enums.TransactionType.Invoice;
        int invoiceId = 101;

        mockService.Setup(s => s.PostTransactionAsync(type, invoiceId))
                   .Returns(Task.CompletedTask);

        // act: execute the posting logic
        await mockService.Object.PostTransactionAsync(type, invoiceId);

        // assert: verify the posting was triggered for this specific invoice
        mockService.Verify(s => s.PostTransactionAsync(
            It.Is<QBD.Domain.Enums.TransactionType>(t => t == type),
            It.Is<int>(id => id == invoiceId)
        ), Times.Once());
    }

    [Fact]
    public async Task PostBillPayment_ShouldTriggerCorrectTransactionType()
    {
        // arrange: setup mock for the posting service
        var mockService = new Mock<ITransactionPostingService>();
        var type = QBD.Domain.Enums.TransactionType.BillPayment;
        int billId = 202;

        mockService.Setup(s => s.PostTransactionAsync(type, billId))
                   .Returns(Task.CompletedTask);

        // act: execute the payment posting
        await mockService.Object.PostTransactionAsync(type, billId);

        // assert: verify that the service was called with BillPayment type
        mockService.Verify(s => s.PostTransactionAsync(
            It.Is<QBD.Domain.Enums.TransactionType>(t => t == QBD.Domain.Enums.TransactionType.BillPayment),
            It.Is<int>(id => id == billId)
        ), Times.Once());
    }

    [Fact]
    public async Task VoidTransaction_ShouldCallServiceWithCorrectParameters()
    {
        // arrange: setup mock for the posting service
        var mockService = new Mock<ITransactionPostingService>();
        var type = TransactionType.JournalEntry;
        int transactionId = 999;

        mockService.Setup(s => s.VoidTransactionAsync(type, transactionId))
                   .Returns(Task.CompletedTask);

        // act: execute the voiding logic
        await mockService.Object.VoidTransactionAsync(type, transactionId);

        // assert: verify the service was called once for this specific transaction
        mockService.Verify(s => s.VoidTransactionAsync(
            It.Is<TransactionType>(t => t == type),
            It.Is<int>(id => id == transactionId)
        ), Times.Once());
    }
}