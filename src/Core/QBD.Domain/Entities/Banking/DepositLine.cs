using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;

namespace QBD.Domain.Entities.Banking;

public class DepositLine : BaseEntity
{
    public int DepositId { get; set; }
    public Deposit Deposit { get; set; } = null!;
    public string? ReceivedFrom { get; set; }
    public int? FromAccountId { get; set; }
    public Account? FromAccount { get; set; }
    public string? Memo { get; set; }
    public int? PaymentMethodId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal Amount { get; set; }
}
