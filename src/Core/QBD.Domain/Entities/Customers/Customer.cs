using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;

namespace QBD.Domain.Entities.Customers;

public class Customer : BaseEntity
{
    public string CustomerName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? BillToAddress { get; set; }
    public string? ShipToAddress { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? TermsId { get; set; }
    public Terms? Terms { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal Balance { get; set; }
    public int? TaxCodeId { get; set; }
    public TaxCode? TaxCode { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
