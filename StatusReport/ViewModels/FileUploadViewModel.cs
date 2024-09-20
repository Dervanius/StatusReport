namespace StatusReport.ViewModels
{
    public class FileUploadViewModel
    {
        //public IFormFile ExcelFile { get; set; }
        //public string SearchType { get; set; } // Barcode or External

        public IFormFile ExcelFile { get; set; }
        public bool SearchByBarcode { get; set; }
        public bool SearchByExternalNumber { get; set; }
    }
}
