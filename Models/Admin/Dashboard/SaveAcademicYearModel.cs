using static BTECH_APP.Enums;

namespace BTECH_APP.Models.Admin.Dashboard
{
    public class SaveAcademicYearModel
    {
        public int AcademicId { get; set; }
        public string SchoolYear { get; set; } = string.Empty;
        public SemesterTypes Semester { get; set; }
        public bool IsActive { get; set; }
    }
}
