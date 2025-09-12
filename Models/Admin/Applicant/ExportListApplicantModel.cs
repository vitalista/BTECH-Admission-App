namespace BTECH_APP.Models.Admin.Applicant
{
    public class ExportListApplicantModel
    {
        public string ApplicantNo { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Birthday { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? SelectedProgram { get; set; }
        public string? AdmittedToProgram { get; set; }
        public DateTime? AddmittedDate { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
