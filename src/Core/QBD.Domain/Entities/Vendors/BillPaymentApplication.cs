using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;

namespace QBD.Domain.Entities.Vendors;

public class BillPaymentApplication : BaseEntity
{
    public int BillPaymentId { get; set; }
    public BillPayment BillPayment { get; set; } = null!;
    public int BillId { get; set; }
    public Bill Bill { get; set; } = null!;
    public decimal AmountApplied { get; set; }
    public decimal DiscountAmount { get; set; }
    public int? DiscountAccountId { get; set; }
    public Account? DiscountAccount { get; set; }
}
