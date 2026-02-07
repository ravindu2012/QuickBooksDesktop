using QBD.Domain.Common;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Banking;

public class ReconciliationLine : BaseEntity
{
    public int ReconciliationId { get; set; }
    public Reconciliation Reconciliation { get; set; } = null!;
    public TransactionType TransactionType { get; set; }
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public bool IsCleared { get; set; }
}
