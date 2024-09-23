using System.ComponentModel.DataAnnotations.Schema;

namespace StatusReport.ViewModels
{
    public class StatusReportViewModel
    {
        public string? Awb { get; set; }

        public string? Nalog { get; set; }

        public string? Barcodes { get; set; }

        public string? ExternalNumber { get; set; }

        public string Status { get; set; } = null!;

        public DateTime EventDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Weight { get; set; }
    }
}
