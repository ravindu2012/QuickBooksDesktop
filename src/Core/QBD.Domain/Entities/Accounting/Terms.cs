// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

using QBD.Domain.Common;

namespace QBD.Domain.Entities.Accounting;

public class Terms : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int DueDays { get; set; }
    public int DiscountDays { get; set; }
    public decimal DiscountPercent { get; set; }
    public bool IsActive { get; set; } = true;
}
