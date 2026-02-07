using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Items;

namespace QBD.Domain.Entities.Vendors;

public class VendorCreditLine : BaseEntity
{
    public int VendorCreditId { get; set; }
    public VendorCredit VendorCredit { get; set; } = null!;
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    public int? ItemId { get; set; }
    public Item? Item { get; set; }
    public decimal Amount { get; set; }
    public string? Memo { get; set; }
    public int? ClassId { get; set; }
    public Class? Class { get; set; }
}
