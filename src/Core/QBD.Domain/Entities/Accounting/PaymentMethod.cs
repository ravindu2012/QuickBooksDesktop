using QBD.Domain.Common;

namespace QBD.Domain.Entities.Accounting;

public class PaymentMethod : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
