using QBD.Domain.Common;

namespace QBD.Domain.Entities.Accounting;

public class JournalEntryLine : BaseEntity
{
    public int JournalEntryId { get; set; }
    public JournalEntry JournalEntry { get; set; } = null!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? Memo { get; set; }
    public int? NameId { get; set; }
    public int? ClassId { get; set; }
    public Class? Class { get; set; }
}
