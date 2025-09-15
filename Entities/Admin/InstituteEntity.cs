using BTECH_APP.Shared.Class;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static BTECH_APP.Enums;

namespace BTECH_APP.Entities.Admin
{
    [Table("institutes")]
    public class InstituteEntity : BaseColumn
    {
        [Key]
        public int InstituteId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Acronym { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalPrograms { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public FileTypes FileType { get; set; }
    }
}
