using QBD.Domain.Common;
using QBD.Domain.Enums;

namespace QBD.Domain.Entities.Customers;

public class Estimate : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string EstimateNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Draft;
    public string? Memo { get; set; }

    public ICollection<EstimateLine> Lines { get; set; } = new List<EstimateLine>();
}
