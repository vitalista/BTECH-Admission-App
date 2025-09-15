using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static BTECH_APP.Enums;

namespace BTECH_APP.Entities.Applicant
{
    [Table("applicants_requirements")]
    public class ApplicantRequirementEntity
    {
        [Key]
        public int ApplicantRequirementId { get; set; }
        public int RequirementId { get; set; }
        public int ApplicantId { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public FileTypes FileType { get; set; }
    }
}
