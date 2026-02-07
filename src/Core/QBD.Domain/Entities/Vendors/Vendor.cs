using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;

namespace QBD.Domain.Entities.Vendors;

public class Vendor : BaseEntity
{
    public string VendorName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? TermsId { get; set; }
    public Terms? Terms { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal Balance { get; set; }
    public string? TaxId { get; set; }
    public bool Is1099 { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
