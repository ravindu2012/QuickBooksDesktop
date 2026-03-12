// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

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