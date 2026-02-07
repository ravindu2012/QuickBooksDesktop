using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Banking;

public class Deposit : BaseEntity
{
    public int BankAccountId { get; set; }
    public Account BankAccount { get; set; } = null!;
    public DateTime Date { get; set; }
    public string? Memo { get; set; }
    public decimal Total { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Draft;

    public ICollection<DepositLine> Lines { get; set; } = new List<DepositLine>();
}
