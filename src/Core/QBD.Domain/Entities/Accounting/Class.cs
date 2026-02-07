using QBD.Domain.Common;

namespace QBD.Domain.Entities.Accounting;

public class Class : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public Class? Parent { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Class> SubClasses { get; set; } = new List<Class>();
}
