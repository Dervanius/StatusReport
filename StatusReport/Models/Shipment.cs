using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StatusReport.Models
{
    public class Shipment
    {
        public int Id { get; set; }
        [StringLength(20)]
        public string? TrackingNumber { get; set; }
        public int CustomerId { get; set; }
        public bool Confirmed { get; set; }
        public bool EditingDisabled { get; set; }
        public bool TrackingDisabled { get; set; }
        public bool ManifestPrinted { get; set; }
        public bool LabelsPrinted { get; set; }
        public bool Canceled { get; set; }
        public int? StatusCodeId { get; set; }
        public int? ReasonCodeId { get; set; }
        public int BillingCodeId { get; set; }
        public int ServiceTypeId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime CollectionDateTimeFrom { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime CollectionDateTimeTo { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DeliveryDateTimeFrom { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DeliveryDateTimeTo { get; set; }
        [StringLength(50)]
        public string ShipFromContactName { get; set; } = null!;
        [StringLength(50)]
        public string ShipFromContactPhone { get; set; } = null!;
        [StringLength(80)]
        public string? ShipFromEmail { get; set; }
        [StringLength(50)]
        public string? ShipFromCompanyName { get; set; }
        [StringLength(100)]
        public string ShipFromStreet1 { get; set; } = null!;
        [StringLength(50)]
        public string? ShipFromStreet2 { get; set; }
        [StringLength(50)]
        public string? ShipFromStreet3 { get; set; }
        [StringLength(20)]
        public string ShipFromPostalCode { get; set; } = null!;
        [StringLength(50)]
        public string ShipFromCity { get; set; } = null!;
        public int ShipFromCountryId { get; set; }
        public int? ShipFromContactTypeId { get; set; }
        [StringLength(50)]
        public string ShipToContactName { get; set; } = null!;
        [StringLength(50)]
        public string ShipToContactPhone { get; set; } = null!;
        [StringLength(80)]
        public string? ShipToEmail { get; set; }
        [StringLength(50)]
        public string? ShipToCompanyName { get; set; }
        [StringLength(100)]
        public string ShipToStreet1 { get; set; } = null!;
        [StringLength(50)]
        public string? ShipToStreet2 { get; set; }
        [StringLength(50)]
        public string? ShipToStreet3 { get; set; }
        [StringLength(20)]
        public string ShipToPostalCode { get; set; } = null!;
        [StringLength(50)]
        public string ShipToCity { get; set; } = null!;
        public int ShipToCountryId { get; set; }
        public int? ShipToContactTypeId { get; set; }
        [StringLength(250)]
        public string? ProductDescription { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GoodsValue { get; set; }
        [StringLength(3)]
        public string GoodsValueCurrencyCode { get; set; } = null!;
        public int ParcelQuantity { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Weight { get; set; }
        public string? PickupNote { get; set; }
        public string? SpecialNote { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime CreatedOn { get; set; }
        [StringLength(50)]
        public string? CreatedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
        [StringLength(50)]
        public string? UpdatedBy { get; set; }
        public int? CostCenterId { get; set; }
        [StringLength(50)]
        public string? DeliveryNote { get; set; }
        public int? PackageTypeId { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal? RansomAmount { get; set; }
        [StringLength(50)]
        public string? ExternalNumber { get; set; }
        public bool DocumentationReturn { get; set; }
        public bool PersonallyHandover { get; set; }
        public bool Fragile { get; set; }
        public bool SpecialPackaging { get; set; }
        public bool ExportFlag { get; set; }
        [StringLength(100)]
        public string? ShippingDocument { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? ConfirmationDate { get; set; }
        public int? StreetIdTo { get; set; }
        public int? StreetIdFrom { get; set; }
        public string? Oid { get; set; }
        public bool? Insurance { get; set; }
        public bool ExportFailed { get; set; }
        public int? UnitOfMeasureId { get; set; }
        [Column(TypeName = "date")]
        public DateTime? CreatedDate { get; set; }
        public bool ConfirmationDisabled { get; set; }
        public bool LinkedShipment { get; set; }
        public int? ParentId { get; set; }
        [StringLength(50)]
        public string? ExternalNumber2 { get; set; }
        public int? ShipmentTypeId { get; set; }
        [Column("ShipmentDCFrom")]
        [StringLength(50)]
        public string? ShipmentDcfrom { get; set; }
        [Column("ShipmentDCTo")]
        [StringLength(50)]
        public string? ShipmentDcto { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? DeliveryDateTime { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? PickUpDateTime { get; set; }
        public int? PriceCategoryId { get; set; }
        public int? ProductTypeId { get; set; }
        public int? ShipFromPostCodeId { get; set; }
        public int? ShipToPostCodeId { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal? Width { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal? Height { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal? Length { get; set; }
        [Column(TypeName = "date")]
        public DateTime? ExportDate { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal? ActualWeight { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? ActualDelivery { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? ActualPickUp { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal? ActualPrice { get; set; }
        public bool SendMail { get; set; }
        public int? ParcelReturnQuantity { get; set; }
        public bool ParcelReturn { get; set; }
        [StringLength(255)]
        public string? LabelsUrl { get; set; }
        [StringLength(5)]
        public string? DistributionCenter { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? StatusTime { get; set; }
        public int? ShipFromZoneId { get; set; }
        public int? ShipToZoneId { get; set; }
        public int? ZoneStatusId { get; set; }
        [StringLength(255)]
        public string? Barcodes { get; set; }
        [StringLength(50)]
        public string? LastMileCarrier { get; set; }
        public bool BadAddress { get; set; }
        public bool BadCity { get; set; }
        public int? ManifestId { get; set; }
        [StringLength(50)]
        public string? MasterBox { get; set; }
        public bool SpremnoZaSlanje { get; set; }
        public bool FlagControl { get; set; }
        public bool? Picked { get; set; }
        [StringLength(255)]
        public string? Barcodes2 { get; set; }
        public int? CourierStatusId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? CourierStatusTime { get; set; }
        public bool CourierTrackingEnabled { get; set; }
        public bool IsCollected { get; set; }

        public bool CarinskoIzuzece { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal? ShipmentVAT { get; set; }
    }
}
