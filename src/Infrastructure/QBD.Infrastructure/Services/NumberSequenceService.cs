using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Domain.Common;
using QBD.Infrastructure.Data;

namespace QBD.Infrastructure.Services;

public class NumberSequenceService : INumberSequenceService
{
    private readonly QBDesktopDbContext _context;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public NumberSequenceService(QBDesktopDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetNextNumberAsync(string entityType)
    {
        await _lock.WaitAsync();
        try
        {
            var sequence = await _context.NumberSequences
                .FirstOrDefaultAsync(s => s.EntityType == entityType);

            if (sequence == null)
            {
                sequence = new NumberSequence
                {
                    EntityType = entityType,
                    Prefix = entityType switch
                    {
                        "Invoice" => "INV-",
                        "Estimate" => "EST-",
                        "SalesReceipt" => "SR-",
                        "CreditMemo" => "CM-",
                        "Bill" => "BILL-",
                        "PurchaseOrder" => "PO-",
                        "Check" => "CHK-",
                        "JournalEntry" => "JE-",
                        "VendorCredit" => "VC-",
                        _ => $"{entityType}-"
                    },
                    NextNumber = 1
                };
                _context.NumberSequences.Add(sequence);
            }

            var number = $"{sequence.Prefix}{sequence.NextNumber:D5}";
            sequence.NextNumber++;
            await _context.SaveChangesAsync();
            return number;
        }
        finally
        {
            _lock.Release();
        }
    }
}
