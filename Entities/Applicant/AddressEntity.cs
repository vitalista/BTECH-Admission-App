using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static BTECH_APP.Enums;

namespace BTECH_APP.Entities.Applicant
{
    [Table("applicants_Address")]
    public class AddressEntity
    {
        [Key]
        public long AddressId { get; set; }
        public long ApplicantId { get; set; }
        public int ProvinceId { get; set; }
        public int MunicipalityId { get; set; }
        public int BarangayId { get; set; }
        public string? Street { get; set; } = string.Empty;
        public string? ZipCode { get; set; } = string.Empty;
        [Column("AddressTypeId")]
        public AddressTypes AddressType { get; set; }
    }
}
