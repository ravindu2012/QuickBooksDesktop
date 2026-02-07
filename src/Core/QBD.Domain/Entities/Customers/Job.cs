using QBD.Domain.Common;

namespace QBD.Domain.Entities.Customers;

public class Job : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string JobName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Awarded, InProgress, Closed
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
