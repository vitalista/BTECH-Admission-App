using BTECH_APP.Helpers;

namespace BTECH_APP.Models.Admin.Applicant
{
    public class ListApplicantModel
    {
        public int ApplicantId { get; set; }
        public string ApplicantNo { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Birthday { get; set; }
        public int Age => Helper.CalculateAge(Birthday);
        public string Gender { get; set; } = string.Empty;
        public List<string> Programs { get; set; } = new();
        public string ApplicantTypeName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
