using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Customers;

public class Invoice : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public int? TermsId { get; set; }
    public Terms? Terms { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Draft;
    public string? Memo { get; set; }
    public string? BillToAddress { get; set; }
    public string? ShipToAddress { get; set; }

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
}
