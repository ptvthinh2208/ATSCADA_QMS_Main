using ATSCADA_Library.DTOs;
using System.Text.Json;

namespace ATSCADA_Library.Services.PrintingService
{


    public class TicketPrinterService
    {

        public List<PrintTemplate> LoadFormItemsFromFile(string path)
        {
            var json = File.ReadAllText(path);
            var items = JsonSerializer.Deserialize<List<PrintTemplate>>(json);
            return items ?? new List<PrintTemplate>();
        }
        public void PrintTicket(PrintRequest request)
        {

            string printerIp = request.PrinterIP; // Địa chỉ IP của máy in POS
            int printerPort = 9100; // Cổng mặc định của máy in
            var formItems = LoadFormItemsFromFile("print_template.json");

            // In form
            var printerService = new PosPrinterService(printerIp, printerPort);
            string date = DateTime.Now.ToString("dd/MM/yyyy");
            string time = DateTime.Now.ToShortTimeString();
            printerService.PrintForm(formItems, request, date, time);
        }


    }

}
