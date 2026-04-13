using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using ATSCADA_Library.Helpers.ExportFile;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json;


namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly ExcelExport _excelExportService = new ExcelExport();

        [HttpPost("ExportToExcel/{entityType}")]
        public IActionResult ExportToExcel(string entityType, [FromBody] JsonElement requestElement)
        {
            if (string.IsNullOrEmpty(entityType))
            {
                return BadRequest("Invalid entity type.");
            }

            byte[] content;
            string nameFile = $"{entityType}.xlsx";

            var rawJson = requestElement.GetRawText();
            Console.WriteLine("JSON Data: " + rawJson);
            switch (entityType.ToLower())
            {
                case "report":
                    var reportRequest = JsonConvert.DeserializeObject<ExportDataRequest<Report>>(rawJson);
                    if (reportRequest == null || !reportRequest.Model.Any())
                        return BadRequest("Invalid data for report export.");
                    content = _excelExportService.ExportToExcel(reportRequest.Model, entityType);
                    break;

                case "queuehistory":
                    var queueHistoriesRequest = JsonConvert.DeserializeObject<ExportDataRequest<QueueHistoryToExportData>>(rawJson);
                    if (queueHistoriesRequest == null || !queueHistoriesRequest.Model.Any())
                        return BadRequest("Invalid data for queue history export.");
                    content = _excelExportService.ExportToExcel(queueHistoriesRequest.Model, queueHistoriesRequest.EntityType);
                    break;
                case "feedback":
                    var feedback = JsonConvert.DeserializeObject<ExportDataRequest<Feedback>>(rawJson);
                    if (feedback == null || !feedback.Model.Any())
                        return BadRequest("Invalid data for queue history export.");
                    content = _excelExportService.ExportToExcel(feedback.Model, feedback.EntityType);
                    break;
                default:
                    return BadRequest("Unsupported entity type.");
            }

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nameFile);
        }
        //[HttpPost("ExportToExcel/Report")]
        //public IActionResult ExportToExcel([FromBody] ExportDataRequest<Report> request)
        //{
        //    // Lấy tên loại đối tượng từ request
        //    var entityType = request.EntityType;
        //    if (string.IsNullOrEmpty(entityType) || request.Model == null || !request.Model.Any())
        //    {
        //        return BadRequest("Invalid entity type or model data.");
        //    }
        //    // Gọi phương thức export generic
        //    var content = _excelExportService.ExportToExcel<Report>(request.Model, entityType);

        //    var nameFile = $"{entityType}.xls";
        //    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nameFile);
        //}
        //[HttpPost("ExportToExcel/ReportDetails")]
        //public IActionResult ExportToExcel([FromBody] ExportDataRequest<ReportDetailsByService> request)
        //{
        //    // Lấy tên loại đối tượng từ request
        //    var entityType = request.EntityType;
        //    if (string.IsNullOrEmpty(entityType) || request.Model == null || !request.Model.Any())
        //    {
        //        return BadRequest("Invalid entity type or model data.");
        //    }
        //    // Gọi phương thức export generic
        //    var content = _excelExportService.ExportToExcel<ReportDetailsByService>(request.Model, entityType);

        //    var nameFile = $"{entityType}.xls";
        //    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nameFile);
        //}
        //[HttpPost("ExportToExcel/Feedback")]
        //public IActionResult ExportToExcel([FromBody] ExportDataRequest<Feedback> request)
        //{
        //    // Lấy tên loại đối tượng từ request
        //    var entityType = request.EntityType;
        //    if (string.IsNullOrEmpty(entityType) || request.Model == null || !request.Model.Any())
        //    {
        //        return BadRequest("Invalid entity type or model data.");
        //    }
        //    // Gọi phương thức export generic
        //    var content = _excelExportService.ExportToExcel<Feedback>(request.Model, entityType);

        //    var nameFile = $"{entityType}.xls";
        //    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nameFile);
        //}
        //[HttpPost("ExportToExcel/QueueHistory")]
        //public IActionResult ExportToExcel([FromBody] ExportDataRequest<QueueHistoryToExportData> request)
        //{
        //    // Lấy tên loại đối tượng từ request
        //    var entityType = request.EntityType;
        //    if (string.IsNullOrEmpty(entityType) || request.Model == null || !request.Model.Any())
        //    {
        //        return BadRequest("Invalid entity type or model data.");
        //    }
        //    // Gọi phương thức export generic
        //    var content = _excelExportService.ExportToExcel<QueueHistoryToExportData>(request.Model, entityType);

        //    var nameFile = $"{entityType}.xls";
        //    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nameFile);
        //}
    }
}
