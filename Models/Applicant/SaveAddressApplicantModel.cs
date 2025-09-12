using static BTECH_APP.Enums;

namespace BTECH_APP.Models.Applicant
{
    public class SaveAddressApplicantModel
    {
        public long AddressId { get; set; }
        public int ProvinceId { get; set; }
        public int MunicipalityId { get; set; }
        public int BarangayId { get; set; }
        public string? Street { get; set; } = string.Empty;
        public string? ZipCode { get; set; } = string.Empty;
        public AddressTypes AddressType { get; set; }
    }
}
