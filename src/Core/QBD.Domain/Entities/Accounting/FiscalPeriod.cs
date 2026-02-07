using QBD.Domain.Common;

namespace QBD.Domain.Entities.Accounting;

public class FiscalPeriod : BaseEntity
{
    public int FiscalYearId { get; set; }
    public FiscalYear FiscalYear { get; set; } = null!;
    public int PeriodNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
    public bool IsAdjusting { get; set; }
}
