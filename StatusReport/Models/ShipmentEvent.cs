namespace StatusReport.Models
{
    public class ShipmentEvent
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }

        public int CustomerId { get; set; }

        public DateTime EventDate { get; set; }

        public int StatusCodeId { get; set; }

        public int ReasonCodeId { get; set; }

        public int ExternalKey { get; set; }

        public DateTime CreatedOn { get; set; }

        public string? DistributionCenter { get; set; }

        public bool ExportFlag { get; set; }
    }
}
