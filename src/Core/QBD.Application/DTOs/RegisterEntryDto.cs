using QBD.Domain.Enums;

namespace QBD.Application.ViewModels;

public class RegisterEntryDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TransactionType TransactionType { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Memo { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
    public bool IsCleared { get; set; }
    public int TransactionId { get; set; }
}
