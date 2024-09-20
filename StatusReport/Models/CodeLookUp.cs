using System.ComponentModel.DataAnnotations;

namespace StatusReport.Models
{
    public class CodeLookUp
    {
        public int Id { get; set; }
        public int CodeListId { get; set; }
        [StringLength(30)]
        public string CodeListName { get; set; } = null!;
        [StringLength(30)]
        public string Code { get; set; } = null!;
        [StringLength(250)]
        public string Description { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public string? Oid { get; set; }
    }
}
