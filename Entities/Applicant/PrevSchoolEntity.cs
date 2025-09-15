using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static BTECH_APP.Enums;

namespace BTECH_APP.Entities.Applicant
{
    [Table("applicant_prevSchool")]
    public class PrevSchoolEntity
    {
        [Key]
        public int SchoolId { get; set; }
        public int ApplicantId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? YearCompleted { get; set; }
        [Column("EducationTypeId")]
        public EducationBackgroundTypes EducationType { get; set; }
    }
}
