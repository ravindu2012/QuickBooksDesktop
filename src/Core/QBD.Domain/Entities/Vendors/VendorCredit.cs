using QBD.Domain.Common;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Vendors;

public class VendorCredit : BaseEntity
{
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public DateTime Date { get; set; }
    public string? RefNo { get; set; }
    public decimal Total { get; set; }
    public decimal BalanceRemaining { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Draft;

    public ICollection<VendorCreditLine> Lines { get; set; } = new List<VendorCreditLine>();
}
