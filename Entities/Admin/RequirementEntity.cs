using BTECH_APP.Shared.Class;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTECH_APP.Entities.Admin
{
    [Table("Requirements")]
    public class RequirementEntity : BaseColumn
    {
        [Key]
        public int RequirementId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsForFreshmen { get; set; }
        public bool IsForTransferee { get; set; }
        public bool IsForAlsGraduate { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
    }
}
