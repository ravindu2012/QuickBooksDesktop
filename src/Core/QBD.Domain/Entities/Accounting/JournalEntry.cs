using QBD.Domain.Common;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Accounting;

public class JournalEntry : BaseEntity
{
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime PostingDate { get; set; }
    public bool IsAdjusting { get; set; }
    public string? Memo { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Draft;

    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}
