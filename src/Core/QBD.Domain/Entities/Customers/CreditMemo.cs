using QBD.Domain.Common;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Customers;

public class CreditMemo : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime Date { get; set; }
    public string CreditNumber { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal BalanceRemaining { get; set; }
    public string? Memo { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Draft;

    public ICollection<CreditMemoLine> Lines { get; set; } = new List<CreditMemoLine>();
}
