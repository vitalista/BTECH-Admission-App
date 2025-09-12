using BTECH_APP.Shared.Class;
using static BTECH_APP.Enums;

namespace BTECH_APP.Models.Admin.Institute
{
    public class ListInstituteModel : ModifiedBy
    {
        public int InstituteId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Acronym { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public FileTypes FileType { get; set; }
    }
}
