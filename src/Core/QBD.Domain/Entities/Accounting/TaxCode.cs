using QBD.Domain.Common;

namespace QBD.Domain.Entities.Accounting;

public class TaxCode : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool IsActive { get; set; } = true;
}
