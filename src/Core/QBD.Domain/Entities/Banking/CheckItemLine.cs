using QBD.Domain.Common;
using QBD.Domain.Entities.Items;

namespace QBD.Domain.Entities.Banking;

public class CheckItemLine : BaseEntity
{
    public int CheckId { get; set; }
    public Check Check { get; set; } = null!;
    public int? ItemId { get; set; }
    public Item? Item { get; set; }
    public string? Description { get; set; }
    public decimal Qty { get; set; }
    public decimal Cost { get; set; }
    public decimal Amount { get; set; }
}
