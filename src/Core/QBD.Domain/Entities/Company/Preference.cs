using QBD.Domain.Common;

namespace QBD.Domain.Entities.Company;

public class Preference : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string Category { get; set; } = string.Empty;
}
