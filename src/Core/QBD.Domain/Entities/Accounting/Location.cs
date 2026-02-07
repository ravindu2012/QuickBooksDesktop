using QBD.Domain.Common;

namespace QBD.Domain.Entities.Accounting;

public class Location : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public Location? Parent { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Location> SubLocations { get; set; } = new List<Location>();
}
