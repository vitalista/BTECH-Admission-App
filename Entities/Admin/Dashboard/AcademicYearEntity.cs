using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static BTECH_APP.Enums;

namespace BTECH_APP.Entities.Admin.Dashboard
{

    [Table("academic_years")]
    public class AcademicYearEntity
    {
        [Key]
        public int AcademicId { get; set; }
        public string SchoolYear { get; set; } = string.Empty;
        public SemesterTypes Semester { get; set; }
        public bool IsActive { get; set; }
    }
}
