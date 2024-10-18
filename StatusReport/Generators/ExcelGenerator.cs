using StatusReport.Information;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Reflection;

namespace StatusReport.Actions
{
    public static class ExcelGenerator
    {

        public static async Task<byte[]> CreateExcelByteArray<T>(List<T> inputList, PropertyInfo[] properties, string worksheetName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(worksheetName);
                PopulateWorksheet(worksheet, inputList, properties);

                byte[] fileBytes = package.GetAsByteArray();
                return fileBytes;
            }
        }

        public static async Task<FileContentResult> CreateExcel<T>(List<T> inputList, PropertyInfo[] properties ,string worksheetName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
           
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(worksheetName);
               
                PopulateWorksheet(worksheet, inputList,properties);

                byte[] fileBytes = package.GetAsByteArray();
                var file = new FileContentResult(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                file.FileDownloadName = worksheetName + ".xlsx";

                return file;
            }
        }
        private static void PopulateWorksheet<T>(ExcelWorksheet worksheet, List<T> inputList, PropertyInfo[] properties)
        {

            // ne moze ovako duplira prvu kolonu tj. kolonu nakon praznog attributa ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            int celijaBroj = 1;
            for (int i = 0; i < properties.Length; i++)
            {
                var excelAttribute = (ExcelExportAttribute)properties[i]
                    .GetCustomAttribute(typeof(ExcelExportAttribute));

                if (excelAttribute == null)
                    continue;

                worksheet.Cells[1, celijaBroj].Value =  excelAttribute.DisplayName; // Set header names
                celijaBroj++;
            }

            // Populate the worksheet with data
            for (int row = 0; row < inputList.Count; row++)
            {
                celijaBroj = 1;
                for (int col = 0; col < properties.Length; col++)
                {
                    var excelAttribute = (ExcelExportAttribute)properties[col]
                    .GetCustomAttribute(typeof(ExcelExportAttribute));

                    if (excelAttribute == null)
                        continue;

                    var value = properties[col].GetValue(inputList[row]);
                    worksheet.Cells[row + 2, celijaBroj].Value = value; // Set cell values
                    celijaBroj++;
                }
            }
        }
    }
}
 
