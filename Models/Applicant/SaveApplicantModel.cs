using BTECH_APP.Models.StaticData;
using static BTECH_APP.Enums;

namespace BTECH_APP.Models.Applicant
{
    public class SaveApplicantModel
    {
        public int ApplicantId { get; set; }
        public int PersonId { get; set; }
        public int UserId { get; set; }
        public bool IsSameAddress { get; set; } = false;
        public string? MobileNo { get; set; } = string.Empty;
        public GenderTypes Gender { get; set; }
        public string? PlaceOfBirth { get; set; } = string.Empty;
        public CivilStatus CivilStatus { get; set; }
        public string? NameOfSpouse { get; set; } = string.Empty;
        public string? MotherName { get; set; } = string.Empty;
        public string? MotherMobileNo { get; set; } = string.Empty;
        public string? FatherName { get; set; } = string.Empty;
        public string? FatherMobileNo { get; set; } = string.Empty;
        public bool IsIndigenous { get; set; } = false;
        public bool IsSoloParent { get; set; } = false;
        public bool IsWithDisabilty { get; set; } = false;
        public bool Is4psMember { get; set; } = false;
        public ApplicantTypes ApplicantType { get; set; }
        public List<SaveAddressApplicantModel>? Address { get; set; }
        public List<SaveSelectedProgApplicantModel>? SelectedPrograms { get; set; }
        public List<SavePrevSchoolApplicantModel>? PrevSchools { get; set; }
        public List<SaveProvinceModel>? PermanetProvinces { get; set; }
        public List<SaveMunicipalityModel>? PermanetMunicipalities { get; set; }
        public List<SaveBarangayModel>? PermanentBarangays { get; set; }
        public List<SaveProvinceModel>? CurrentProvinces { get; set; }
        public List<SaveMunicipalityModel>? CurrentMunicipalities { get; set; }
        public List<SaveBarangayModel>? CurrentBarangays { get; set; }
    }
}
