using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Items;

namespace QBD.Domain.Entities.Customers;

public class InvoiceLine : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public int? ItemId { get; set; }
    public Item? Item { get; set; }
    public string? Description { get; set; }
    public decimal Qty { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public int? TaxCodeId { get; set; }
    public TaxCode? TaxCode { get; set; }
    public int? ClassId { get; set; }
    public Class? Class { get; set; }
}
