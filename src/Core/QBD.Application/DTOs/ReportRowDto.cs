namespace QBD.Application.ViewModels;

public class ReportRowDto
{
    public int Level { get; set; }
    public string Label { get; set; } = string.Empty;
    public Dictionary<string, object?> Values { get; set; } = new();
    public bool IsBold { get; set; }
    public bool IsTotal { get; set; }
    public bool IsSeparator { get; set; }
    public int? EntityId { get; set; }
    public string? EntityType { get; set; }
    public List<ReportRowDto> Children { get; set; } = new();
}
