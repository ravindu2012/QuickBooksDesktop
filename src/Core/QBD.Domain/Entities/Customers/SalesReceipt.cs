using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Customers;

public class SalesReceipt : BaseEntity
{
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime Date { get; set; }
    public int? PaymentMethodId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public int? DepositToAccountId { get; set; }
    public Account? DepositToAccount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string? Memo { get; set; }
    public string SalesReceiptNumber { get; set; } = string.Empty;
    public DocStatus Status { get; set; } = DocStatus.Draft;

    public ICollection<SalesReceiptLine> Lines { get; set; } = new List<SalesReceiptLine>();
}
