namespace StatusReport.Models.Dto
{
    public class SpecificationNeededStatuses
    {
        public int ShipmentId { get; set; }
        public string ExternalNumber { get; set; }
        public string Barcodes { get; set; }
        public string PoslednjiStatus { get; set; }
        public DateTime DatumPoslednjegStatusa { get; set; }

        public DateTime? DatumNaCarinjenju { get; set; }
        public DateTime? DatumOcarinjeno { get; set; }
        public DateTime? DatumPredatoKuriru { get; set; }
        public DateTime? DatumUDisCentr { get; set; }

        public string StatusKurira { get; set; }
        public DateTime? DatumStatusKurir { get; set; }

        public DateTime? DatumIsporucen { get; set; }

    }
}
