using static BTECH_APP.Enums;

namespace BTECH_APP.Models.Applicant
{
    public class SaveRequirementsApplicantModel
    {
        public int ApplicantRequirementId { get; set; }
        public int RequirementId { get; set; }
        public string? Name { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public FileTypes FileType { get; set; }
        public bool IsRequired { get; set; } = false;
    }
}
