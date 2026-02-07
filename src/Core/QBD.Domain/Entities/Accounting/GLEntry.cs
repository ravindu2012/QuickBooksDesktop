using QBD.Domain.Common;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Accounting;

public class GLEntry : BaseEntity
{
    public DateTime PostingDate { get; set; }
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal RunningBalance { get; set; }
    public TransactionType TransactionType { get; set; }
    public int TransactionId { get; set; }
    public string? TransactionNumber { get; set; }
    public string? Memo { get; set; }
    public string? NameType { get; set; }
    public int? NameId { get; set; }
    public string? NameDisplay { get; set; }
    public int? ClassId { get; set; }
    public Class? Class { get; set; }
    public int? LocationId { get; set; }
    public Location? Location { get; set; }
    public int? FiscalPeriodId { get; set; }
    public FiscalPeriod? FiscalPeriod { get; set; }
    public bool IsVoid { get; set; }
}
