using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Vendors;

public class Bill : BaseEntity
{
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public string? BillNumber { get; set; }
    public string? VendorRefNo { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public int? TermsId { get; set; }
    public Terms? Terms { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Draft;
    public string? Memo { get; set; }

    public ICollection<BillExpenseLine> ExpenseLines { get; set; } = new List<BillExpenseLine>();
    public ICollection<BillItemLine> ItemLines { get; set; } = new List<BillItemLine>();
}
