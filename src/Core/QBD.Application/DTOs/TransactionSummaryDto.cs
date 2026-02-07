using QBD.Domain.Enums;

namespace QBD.Application.ViewModels;

public class TransactionSummaryDto
{
    public int Id { get; set; }
    public TransactionType Type { get; set; }
    public string TypeDisplay { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string? Memo { get; set; }
    public DocStatus Status { get; set; }
}
