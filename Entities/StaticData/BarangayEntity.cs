using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTECH_APP.Entities.StaticData
{
    [Table("barangays")]
    public class BarangayEntity
    {
        [Key]
        public int BarangayId { get; set; }
        public int MunicipalityId { get; set; }
        public string? Name { get; set; }
    }
}
