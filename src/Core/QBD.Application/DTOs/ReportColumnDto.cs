namespace QBD.Application.ViewModels;

public class ReportColumnDto
{
    public string Header { get; set; } = string.Empty;
    public string BindingPath { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public double Width { get; set; } = 100;
    public string Alignment { get; set; } = "Left";
}
