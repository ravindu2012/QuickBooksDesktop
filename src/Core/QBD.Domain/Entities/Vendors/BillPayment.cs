using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Vendors;

public class BillPayment : BaseEntity
{
    public DateTime Date { get; set; }
    public int PaymentAccountId { get; set; }
    public Account PaymentAccount { get; set; } = null!;
    public int? PaymentMethodId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? Memo { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Draft;

    public ICollection<BillPaymentApplication> Applications { get; set; } = new List<BillPaymentApplication>();
}
