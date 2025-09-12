using BTECH_APP.Models.Applicant;

namespace BTECH_APP.Models.Admin.Applicant
{
    public class AdmisisionFormApplicantModel
    {
        public int ApplicantId { get; set; }
        public int PersonId { get; set; }
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Suffix { get; set; }
        public string? BirthDate { get; set; }
        public string? ApplicantNo { get; set; }
        public bool IsSameAddress { get; set; } = false;
        public string? MobileNo { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? PlaceOfBirth { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? CivilStatus { get; set; }
        public string? NameOfSpouse { get; set; } = string.Empty;
        public string? MotherName { get; set; } = string.Empty;
        public string? MotherMobileNo { get; set; } = string.Empty;
        public string? FatherName { get; set; } = string.Empty;
        public string? FatherMobileNo { get; set; } = string.Empty;
        public string? IsIndigenous { get; set; }
        public string? IsSoloParent { get; set; }
        public string? IsWithDisabilty { get; set; }
        public string? Is4psMember { get; set; }
        public string? ApplicantType { get; set; } = string.Empty;
        public string? PermenentAddress { get; set; } = string.Empty;
        public string? PresentAddress { get; set; } = string.Empty;
        public string? FirstChoice { get; set; } = string.Empty;
        public string? SecondChoice { get; set; } = string.Empty;
        public List<SavePrevSchoolApplicantModel> PrevSchools { get; set; } = new();
        public List<SaveRequirementsApplicantModel> Requirements { get; set; } = new();
    }
}
