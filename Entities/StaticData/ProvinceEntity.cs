using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTECH_APP.Entities.StaticData
{
    [Table("Provinces")]
    public class ProvinceEntity
    {
        [Key]
        public int ProvinceId { get; set; }
        public string? Name { get; set; }
    }
}
