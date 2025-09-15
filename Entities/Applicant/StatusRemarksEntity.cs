using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static BTECH_APP.Enums;

namespace BTECH_APP.Entities.Applicant
{
    [Table("applicants_Status_Remarks")]
    public class StatusRemarksEntity
    {
        [Key]
        public long RemarksId { get; set; }
        public long ApplicantId { get; set; }
        [Column("StatusId")]
        public ApplicantStatus Status { get; set; }
        public string? Description { get; set; }
        public string? Remarks { get; set; }
        public DateTime? Date { get; set; }
    }
}
