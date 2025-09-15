using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static BTECH_APP.Enums;

namespace BTECH_APP.Entities.Applicant
{
    [Table("applicants_Selected_Programs")]
    public class SelectedProgramEntity
    {
        [Key]
        public int SelectedProgramId { get; set; }
        public int ApplicantId { get; set; }
        public int ProgramId { get; set; }
        [Column("SelectedProgramTypeId")]
        public SelectedProgramTypes SelectedProgramType { get; set; }
    }
}
