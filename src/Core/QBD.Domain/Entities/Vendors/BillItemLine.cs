using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Customers;
using QBD.Domain.Entities.Items;

namespace QBD.Domain.Entities.Vendors;

public class BillItemLine : BaseEntity
{
    public int BillId { get; set; }
    public Bill Bill { get; set; } = null!;
    public int? ItemId { get; set; }
    public Item? Item { get; set; }
    public string? Description { get; set; }
    public decimal Qty { get; set; }
    public decimal Cost { get; set; }
    public decimal Amount { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? ClassId { get; set; }
    public Class? Class { get; set; }
}
