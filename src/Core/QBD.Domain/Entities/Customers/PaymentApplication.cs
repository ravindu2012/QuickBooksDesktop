using QBD.Domain.Common;

namespace QBD.Domain.Entities.Customers;

public class PaymentApplication : BaseEntity
{
    public int ReceivePaymentId { get; set; }
    public ReceivePayment ReceivePayment { get; set; } = null!;
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public decimal AmountApplied { get; set; }
}
