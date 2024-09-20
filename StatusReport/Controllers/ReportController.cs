using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using StatusReport.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using StatusReport.Services;
using Newtonsoft.Json;

namespace StatusReport.Controllers
{
    public class ReportController : Controller
    {
        private readonly ReportServices _reportServices;

        public ReportController()
        {
            _reportServices = new ReportServices();
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcelFile(FileUploadViewModel model, string optradio)
        {
            if (model.ExcelFile == null || model.ExcelFile.Length == 0)
            {
                ViewData["ErrorMessage"] = "Please upload a non-empty Excel file.";
                return View("Index", model);
            }

            var extension = Path.GetExtension(model.ExcelFile.FileName);
            if (extension != ".xls" && extension != ".xlsx")
            {
                ViewData["ErrorMessage"] = "Invalid file format. Please upload an Excel file.";
                return View("Index", model);
            }

            List<string> barcodes = new List<string>();
            using (var stream = new MemoryStream())
            {
                await model.ExcelFile.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    if (worksheet.Dimension == null || worksheet.Dimension.Rows == 0)
                    {
                        ViewData["ErrorMessage"] = "The uploaded Excel file is empty.";
                        return View("Index", model);
                    }

                    // Read values from Column A (assuming barcodes are there)
                    for (int row = 1; row <= worksheet.Dimension.Rows; row++)
                    {
                        var barcode = worksheet.Cells[row, 1].Text;
                        if (!string.IsNullOrEmpty(barcode))
                        {
                            barcodes.Add(barcode);
                        }
                    }
                }
            }

            if (optradio == "barcode")
            {
                // Convert list of barcodes to JSON
                var jsonList = JsonConvert.SerializeObject(barcodes.Select(b => new { Barcodes = b }));

                // Execute the query using the service
                var reportResults = await _reportServices.GetReportByBarcodesAsync(jsonList);

                // Generate the result Excel file
                return await GenerateExcelFile(reportResults);
            }
            else //external
            {
                // Convert list of barcodes to JSON
                var jsonList = JsonConvert.SerializeObject(barcodes);

                // Execute the query using the service
                var reportResults = await _reportServices.GetReportByExternalNumbersAsync(jsonList);

                // Generate the result Excel file
                return await GenerateExcelFileByExtNo(reportResults);

            }
        }


        private async Task<IActionResult> GenerateExcelFile(IEnumerable<dynamic> reportResults)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                // Adding header row
                worksheet.Cells[1, 1].Value = "Barcode";
                worksheet.Cells[1, 2].Value = "Event ID";
                worksheet.Cells[1, 3].Value = "Event Date";
                worksheet.Cells[1, 4].Value = "Status Code";
                worksheet.Cells[1, 5].Value = "Description";

                // Adding data rows
                int row = 2;
                foreach (var result in reportResults)
                {
                    worksheet.Cells[row, 1].Value = result.Barcodes;
                    worksheet.Cells[row, 2].Value = result.EventId;
                    worksheet.Cells[row, 3].Value = result.EventDate;
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "yyyy-mm-dd hh-mm-ss";
                    worksheet.Cells[row, 4].Value = result.StatusCodeId;
                    worksheet.Cells[row, 5].Value = result.Description;
                    row++;
                }

                // Save the Excel package
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"StatusReport_{DateTime.Now.ToString("ddMMyyyy_HHmm")}.xlsx";

                // Return Excel file as download
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        private async Task<IActionResult> GenerateExcelFileByExtNo(IEnumerable<dynamic> reportResults)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                // Adding header row
                worksheet.Cells[1, 1].Value = "Barcode";
                worksheet.Cells[1, 2].Value = "ExternalNumber";
                worksheet.Cells[1, 3].Value = "Status";
                worksheet.Cells[1, 4].Value = "EventDate";
                worksheet.Cells[1, 5].Value = "Weight";
                worksheet.Cells[1, 6].Value = "AWB";

                // Adding data rows
                int row = 2;
                foreach (var result in reportResults)
                {
                    worksheet.Cells[row, 1].Value = result.Barcodes;
                    worksheet.Cells[row, 2].Value = result.ExternalNumber;
                    worksheet.Cells[row, 3].Value = result.Status;
                    worksheet.Cells[row, 4].Value = result.EventDate;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "yyyy-mm-dd hh-mm-ss";
                    worksheet.Cells[row, 5].Value = result.weight;
                    worksheet.Cells[row, 6].Value = result.awb;
                    row++;
                }

                // Save the Excel package
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"StatusReport_{DateTime.Now.ToString("ddMMyyyy_HHmm")}.xlsx";

                // Return Excel file as download
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }



        public IActionResult Index()
        {
            return View();
        }
    }
}
