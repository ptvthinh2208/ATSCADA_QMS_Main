using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;

namespace ATSCADA_Client.Interfaces.Client
{
    public interface IExportApiClient
    {
        //Hàm export generic
        Task<byte[]> ExportToExcel<T>(ExportDataRequest<T> request);
        Task<byte[]> ExportToExcel(ExportDataRequest<Report> request);
        //Task<byte[]> ExportToExcel(ExportDataRequest<ReportDetailsByService> request);
        Task<byte[]> ExportToExcel(ExportDataRequest<Feedback> request);
        Task<byte[]> ExportToExcel(ExportDataRequest<QueueHistoryToExportData> request);
    }
}
