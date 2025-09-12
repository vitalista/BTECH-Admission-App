using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTECH_APP.Entities.StaticData
{
    [Table("Municipalities")]
    public class MunicipalityEntity
    {
        [Key]
        public int MunicipalityId { get; set; }
        public int ProvinceId { get; set; }
        public string? Name { get; set; }
    }
}
