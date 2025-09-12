using static BTECH_APP.Enums;

namespace BTECH_APP.Models.Applicant
{
    public class SavePrevSchoolApplicantModel
    {
        public int SchoolId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? YearCompleted { get; set; }
        public EducationBackgroundTypes EducationType { get; set; }
    }
}
