using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Banking;

public class Check : BaseEntity
{
    public int BankAccountId { get; set; }
    public Account BankAccount { get; set; } = null!;
    public string? PayToName { get; set; }
    public string? PayToType { get; set; }
    public int? PayToId { get; set; }
    public string? CheckNumber { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public bool IsToBePrinted { get; set; }
    public string? Memo { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Draft;

    public ICollection<CheckExpenseLine> ExpenseLines { get; set; } = new List<CheckExpenseLine>();
    public ICollection<CheckItemLine> ItemLines { get; set; } = new List<CheckItemLine>();
}
