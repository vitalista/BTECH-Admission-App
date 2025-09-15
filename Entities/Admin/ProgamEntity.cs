using BTECH_APP.Shared.Class;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTECH_APP.Entities.Admin
{
    [Table("programs")]
    public class ProgamEntity : BaseColumn
    {
        [Key]
        public int ProgramId { get; set; }
        public int InstituteId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Acronym { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
