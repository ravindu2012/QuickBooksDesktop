using System.Threading.Tasks;

namespace QBD.Application.Interfaces
{
    public interface IPdfExportService
    {
        Task ExportInvoiceToPdfAsync(object invoiceData, string filePath);
        
        Task ExportReportToPdfAsync(string reportTitle, object reportData, string filePath);
    }
}