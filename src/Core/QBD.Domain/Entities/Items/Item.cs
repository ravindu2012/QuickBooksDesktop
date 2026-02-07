using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Items;

public class Item : BaseEntity
{
    public string ItemName { get; set; } = string.Empty;
    public ItemType ItemType { get; set; }
    public string? Description { get; set; }
    public decimal SalesPrice { get; set; }
    public decimal PurchaseCost { get; set; }
    public int? IncomeAccountId { get; set; }
    public Account? IncomeAccount { get; set; }
    public int? ExpenseAccountId { get; set; }
    public Account? ExpenseAccount { get; set; }
    public int? AssetAccountId { get; set; }
    public Account? AssetAccount { get; set; }
    public decimal QtyOnHand { get; set; }
    public decimal ReorderPoint { get; set; }
    public bool IsActive { get; set; } = true;
    public int? TaxCodeId { get; set; }
    public TaxCode? TaxCode { get; set; }
    public int? ParentItemId { get; set; }
    public Item? ParentItem { get; set; }

    public ICollection<Item> SubItems { get; set; } = new List<Item>();
}
