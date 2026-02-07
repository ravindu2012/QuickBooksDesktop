using QBD.Domain.Common;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Accounting;

public class Account : BaseEntity
{
    public string? Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public int? ParentId { get; set; }
    public Account? Parent { get; set; }
    public bool IsSubAccount { get; set; }
    public int Depth { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDebitNormal { get; set; }
    public bool IsSystemAccount { get; set; }
    public decimal OpeningBalance { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }

    public ICollection<Account> SubAccounts { get; set; } = new List<Account>();
    public ICollection<GLEntry> GLEntries { get; set; } = new List<GLEntry>();
}
