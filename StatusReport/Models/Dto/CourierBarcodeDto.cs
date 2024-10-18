using StatusReport.Information;

namespace StatusReport.Models.Dto
{
    public class CourierBarcodeDto
    {
        [ExcelExport("Naziv Kurira")]
        public string Kurir { get; set; }

        [ExcelExport("TSF Barcode")]
        public string TsfBarcode { get; set; }

        [ExcelExport("Courier Barcode")]
        public string KurirBarcode { get; set; }

    }
}
