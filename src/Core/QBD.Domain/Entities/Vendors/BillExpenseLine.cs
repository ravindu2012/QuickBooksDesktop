using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Customers;

namespace QBD.Domain.Entities.Vendors;

public class BillExpenseLine : BaseEntity
{
    public int BillId { get; set; }
    public Bill Bill { get; set; } = null!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public decimal Amount { get; set; }
    public string? Memo { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? ClassId { get; set; }
    public Class? Class { get; set; }
}
