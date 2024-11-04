using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using StatusReport.Actions;
using StatusReport.Models.Dto;
using StatusReport.Services;
using StatusReport.ViewModels;

namespace StatusReport.Controllers
{
    public class EcomController : Controller
    {
        private readonly ReportServices _reportServices;
        private readonly BarcodeServices _barcodeServices;

        public EcomController()
        {
            _reportServices = new ReportServices();
            _barcodeServices = new BarcodeServices();
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcelFile(FileUploadViewModel model, string criteria, string status)
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

                    // Read values from Column A
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


            if (criteria == "barcode" && status == "cleared")
            {

                var jsonList = JsonConvert.SerializeObject(barcodes);

                var reportResults = await _reportServices.GetClearedReport(jsonList, true);

                return await GenerateCustomsPdvExcel(reportResults);
            }
            else if (criteria == "external" && status == "cleared")
            {

                var jsonList = JsonConvert.SerializeObject(barcodes);

                var reportResults = await _reportServices.GetClearedReport(jsonList, false);

                return await GenerateCustomsPdvExcel(reportResults);

            }
            else if (criteria == "barcode" && status == "last")
            {
                var jsonList = JsonConvert.SerializeObject(barcodes);

                var reportResults = await _reportServices.GetLastReport(jsonList, true);

                return await GenerateExcelFile(reportResults);
            }
            else if (criteria == "external" && status == "last")
            {
                var jsonList = JsonConvert.SerializeObject(barcodes);

                var reportResults = await _reportServices.GetLastReport(jsonList, false);

                return await GenerateExcelFile(reportResults);
            }
            else if (criteria == "barcode" && status == "temporary")
            {
                var jsonList = JsonConvert.SerializeObject(barcodes);

                var reportResults = await _reportServices.GetTemporaryReport(jsonList, true);

                return await GenerateExcelFile(reportResults);
            }
            else if (criteria == "barcode" && status == "barcode")
            {
                var reportResults = await _barcodeServices.GetCourierBarcode(barcodes, true);
                return await GenerateCourierBarcodeExcel(reportResults);
            }
            else if (criteria == "external" && status == "barcode")
            {
                var reportResults = await _barcodeServices.GetCourierBarcode(barcodes, false);
                return await GenerateCourierBarcodeExcel(reportResults);
            }
            else if (criteria == "barcode" && status == "LMC")
            {
                var jsonList = JsonConvert.SerializeObject(barcodes);
                var reportResults = await _reportServices.GetLMCStatuses(jsonList, true);
                return await GenerateLMCExcel(reportResults);
            }
            else if (criteria == "external" && status == "LMC")
            {
                var jsonList = JsonConvert.SerializeObject(barcodes);
                var reportResults = await _reportServices.GetLMCStatuses(jsonList, false);
                return await GenerateLMCExcel(reportResults);
            }
            else
            {
                var jsonList = JsonConvert.SerializeObject(barcodes);

                var reportResults = await _reportServices.GetTemporaryReport(jsonList, false);

                return await GenerateExcelFile(reportResults);
            }
        }

        private async Task<IActionResult> GenerateExcelFile(IEnumerable<dynamic> reportResults)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                // Adding header row
                worksheet.Cells[1, 1].Value = "AWB";
                worksheet.Cells[1, 2].Value = "Nalog";
                worksheet.Cells[1, 3].Value = "Last Mile Carrier";
                worksheet.Cells[1, 4].Value = "Barcode";
                worksheet.Cells[1, 5].Value = "External Number";
                worksheet.Cells[1, 6].Value = "Status";
                worksheet.Cells[1, 7].Value = "Status Date";
                worksheet.Cells[1, 8].Value = "Status Time";
                worksheet.Cells[1, 9].Value = "Created On Date";
                worksheet.Cells[1, 10].Value = "Created On Time";
                worksheet.Cells[1, 11].Value = "Weight";
                worksheet.Cells[1, 12].Value = "GoodsValue";
                worksheet.Cells[1, 13].Value = "VAT";


                // Adding data rows
                int row = 2;
                foreach (var result in reportResults)
                {
                    worksheet.Cells[row, 1].Value = result.AWB;
                    worksheet.Cells[row, 2].Value = result.Nalog;
                    worksheet.Cells[row, 3].Value = result.LastMileCarrier;
                    worksheet.Cells[row, 4].Value = result.Barcodes;
                    worksheet.Cells[row, 5].Value = result.ExternalNumber;
                    worksheet.Cells[row, 6].Value = result.Status;
                    worksheet.Cells[row, 7].Value = result.EventDateOnly;
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "yyyy.mm.dd";
                    worksheet.Cells[row, 8].Value = result.EventDate;
                    worksheet.Cells[row, 8].Style.Numberformat.Format = "hh:mm:ss";
                    worksheet.Cells[row, 9].Value = result.CreatedOnDateOnly;
                    worksheet.Cells[row, 9].Style.Numberformat.Format = "yyyy.mm.dd";
                    worksheet.Cells[row, 10].Value = result.CreatedOn;
                    worksheet.Cells[row, 10].Style.Numberformat.Format = "hh:mm:ss";
                    worksheet.Cells[row, 11].Value = result.Weight;
                    worksheet.Cells[row, 12].Value = result.GoodsValue;
                    worksheet.Cells[row, 13].Value = result.GoodsValue * 0.2m;

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

        private async Task<IActionResult> GenerateCustomsPdvExcel(IEnumerable<SpecificationNeededStatuses> statusi)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                // Adding header row
                worksheet.Cells[1, 1].Value = "ShipmentId";
                worksheet.Cells[1, 2].Value = "ExternalNumber";
                worksheet.Cells[1, 3].Value = "Barcodes";
                worksheet.Cells[1, 4].Value = "PoslednjiStatus";
                worksheet.Cells[1, 5].Value = "DatumPoslednjegStatusa";
                worksheet.Cells[1, 6].Value = "DatumNaCarinjenju";
                worksheet.Cells[1, 7].Value = "DatumOcarinjeno";
                worksheet.Cells[1, 8].Value = "DatumPredatoKuriru";
                worksheet.Cells[1, 9].Value = "DatumUDisCentr";
                worksheet.Cells[1, 10].Value = "StatusKurira";
                worksheet.Cells[1, 11].Value = "DatumStatusKurir";

                // Adding data rows
                int row = 2;
                foreach (var status in statusi)
                {
                    worksheet.Cells[row, 1].Value = status.ShipmentId;
                    worksheet.Cells[row, 2].Value = status.ExternalNumber;
                    worksheet.Cells[row, 3].Value = status.Barcodes;
                    worksheet.Cells[row, 4].Value = status.PoslednjiStatus;
                    worksheet.Cells[row, 5].Value = status.DatumPoslednjegStatusa;
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "yyyy.mm.dd hh:mm:ss";

                    worksheet.Cells[row, 6].Value = status.DatumNaCarinjenju;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "yyyy.mm.dd hh:mm:ss";

                    worksheet.Cells[row, 7].Value = status.DatumOcarinjeno;
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "yyyy.mm.dd hh:mm:ss";

                    worksheet.Cells[row, 8].Value = status.DatumPredatoKuriru;
                    worksheet.Cells[row, 8].Style.Numberformat.Format = "yyyy.mm.dd hh:mm:ss";


                    worksheet.Cells[row, 9].Value = status.DatumUDisCentr;
                    worksheet.Cells[row, 9].Style.Numberformat.Format = "yyyy.mm.dd hh:mm:ss";

                    worksheet.Cells[row, 10].Value = status.StatusKurira;

                    worksheet.Cells[row, 11].Value = status.DatumStatusKurir;
                    worksheet.Cells[row, 11].Style.Numberformat.Format = "yyyy.mm.dd hh:mm:ss";


                    row++;
                }

                // Save the Excel package
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Customs-PDV_{DateTime.Now.ToString("ddMMyyyy_HHmm")}.xlsx";

                // Return Excel file as download
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        private async Task<IActionResult> GenerateLMCExcel(IEnumerable<SpecificationNeededStatuses> statusi)
        {
            throw new NotImplementedException();
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                // Adding header row
                worksheet.Cells[1, 1].Value = "ShipmentId";
                worksheet.Cells[1, 2].Value = "ExternalNumber";
                worksheet.Cells[1, 3].Value = "Barcodes";
                worksheet.Cells[1, 4].Value = "PoslednjiStatus";
                worksheet.Cells[1, 5].Value = "DatumPoslednjegStatusa";
                worksheet.Cells[1, 6].Value = "DatumNaCarinjenju";
                worksheet.Cells[1, 7].Value = "DatumOcarinjeno";
                worksheet.Cells[1, 8].Value = "DatumPredatoKuriru";
                worksheet.Cells[1, 9].Value = "DatumUDisCentr";
                worksheet.Cells[1, 10].Value = "StatusKurira";
                worksheet.Cells[1, 11].Value = "DatumStatusKurir";

                // Adding data rows
                int row = 2;
                foreach (var status in statusi)
                {
                    worksheet.Cells[row, 1].Value = status.ShipmentId;
                    worksheet.Cells[row, 2].Value = status.ExternalNumber;
                    worksheet.Cells[row, 3].Value = status.Barcodes;
                    worksheet.Cells[row, 4].Value = status.PoslednjiStatus;
                    worksheet.Cells[row, 5].Value = status.DatumPoslednjegStatusa;
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "yyyy.mm.dd hh:mm:ss";

                    worksheet.Cells[row, 6].Value = status.DatumNaCarinjenju;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "yyyy.mm.dd hh:mm:ss";

                    worksheet.Cells[row, 7].Value = status.DatumOcarinjeno;
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "yyyy.mm.dd hh:mm:ss";

                    worksheet.Cells[row, 8].Value = status.DatumPredatoKuriru;
                    worksheet.Cells[row, 8].Style.Numberformat.Format = "yyyy.mm.dd hh:mm:ss";


                    worksheet.Cells[row, 9].Value = status.DatumUDisCentr;
                    worksheet.Cells[row, 9].Style.Numberformat.Format = "yyyy.mm.dd hh:mm:ss";

                    worksheet.Cells[row, 10].Value = status.StatusKurira;

                    worksheet.Cells[row, 11].Value = status.DatumStatusKurir;
                    worksheet.Cells[row, 11].Style.Numberformat.Format = "yyyy.mm.dd hh:mm:ss";


                    row++;
                }

                // Save the Excel package
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Customs-PDV_{DateTime.Now.ToString("ddMMyyyy_HHmm")}.xlsx";

                // Return Excel file as download
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
        private async Task<IActionResult> GenerateCourierBarcodeExcel(List<CourierBarcodeDto> listCourierBarcodeDto)
        {
            var excel = await ExcelGenerator.CreateExcel(listCourierBarcodeDto, typeof(CourierBarcodeDto).GetProperties(), $"Barcodes_{DateTime.Now.ToString("yyMMddHHmmss")}");
            if (excel == null)
                return null;

            return excel;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
