using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Banking;

public class Transfer : BaseEntity
{
    public int FromAccountId { get; set; }
    public Account FromAccount { get; set; } = null!;
    public int ToAccountId { get; set; }
    public Account ToAccount { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? Memo { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Draft;
}
