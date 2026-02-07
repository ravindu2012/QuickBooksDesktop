using QBD.Domain.Common;
using QBD.Domain.Entities.Items;

namespace QBD.Domain.Entities.Vendors;

public class PurchaseOrderLine : BaseEntity
{
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public int? ItemId { get; set; }
    public Item? Item { get; set; }
    public string? Description { get; set; }
    public decimal Qty { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
}
