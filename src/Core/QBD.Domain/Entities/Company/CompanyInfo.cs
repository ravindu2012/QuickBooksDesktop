using QBD.Domain.Common;

namespace QBD.Domain.Entities.Company;

public class CompanyInfo : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? EIN { get; set; }
    public int FiscalYearStartMonth { get; set; } = 1;
}
