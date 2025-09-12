using static BTECH_APP.Enums;

namespace BTECH_APP.Models.Admin.Institute
{
    public class SaveInstituteModel
    {
        public int InstituteId { get; set; }
        public string? Name { get; set; }
        public string? Acronym { get; set; }
        public bool IsActive { get; set; } = true;
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public FileTypes FileType { get; set; }
    }
}
