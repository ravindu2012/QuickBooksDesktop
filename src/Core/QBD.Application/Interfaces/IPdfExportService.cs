using System.Threading.Tasks;
using QBD.Domain.Entities.Customers;

namespace QBD.Application.Interfaces
{
    public interface IPdfExportService
    {
        Task ExportInvoiceToPdfAsync(Invoice invoice, string filePath);

        Task ExportReportToPdfAsync(string reportTitle, object reportData, string filePath);
    }
}