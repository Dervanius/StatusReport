using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StatusReport.Models
{
    public class Manifest
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("Manifest")]
        [StringLength(50)]
        public string Manifest1 { get; set; } = null!;

        public int? ManifestStatusId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }

        [Column("ETA", TypeName = "datetime")]
        public DateTime? Eta { get; set; }

        [Column("DateAtRegionalHUB", TypeName = "datetime")]
        public DateTime? DateAtRegionalHub { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DatePickUp { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DateAtLocalHub { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DateCustomsStarted { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DateCustomsCompleted { get; set; }

        [StringLength(80)]
        public string? Shipper { get; set; }

        [StringLength(100)]
        public string? ShipperAddress { get; set; }

        [Column("AWB")]
        [StringLength(30)]

        public string? Awb { get; set; }

        [Column("MRN")]
        [StringLength(20)]
        public string? Mrn { get; set; }

        [StringLength(20)]
        public string? Nalog { get; set; }

        [Column("GrossWeightMRN", TypeName = "decimal(10, 2)")]
        public decimal? GrossWeightMrn { get; set; }

        public int? Colli { get; set; }

        public int? Quantity { get; set; }

        [Column("ValueRSD", TypeName = "decimal(14, 2)")]
        public decimal? ValueRsd { get; set; }

        public int RB { get; set; }

        [StringLength(30)]
        public string? Profaktura { get; set; }

        [StringLength(30)]
        public string? PrevoznoSredstvo { get; set; }


        public DateTime DatePredatoKuriru { get; set; }

    }
}
