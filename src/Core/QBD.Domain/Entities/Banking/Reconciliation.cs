using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;

namespace QBD.Domain.Entities.Banking;

public class Reconciliation : BaseEntity
{
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public DateTime StatementDate { get; set; }
    public decimal EndingBalance { get; set; }
    public bool IsCompleted { get; set; }

    public ICollection<ReconciliationLine> Lines { get; set; } = new List<ReconciliationLine>();
}
