namespace QBD.Domain.Common;

public class NumberSequence : BaseEntity
{
    public string EntityType { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public int NextNumber { get; set; } = 1;
}
