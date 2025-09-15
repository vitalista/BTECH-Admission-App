using BTECH_APP.Shared.Class;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static BTECH_APP.Enums;

namespace BTECH_APP.Entities.Applicant
{
    [Table("applicants")]
    public class ApplicantEntity : BaseColumn
    {
        [Key]
        public int ApplicantId { get; set; }
        public int UserId { get; set; }
        public int PersonId { get; set; }
        public string? ApplicantNo { get; set; }
        public bool IsSameAddress { get; set; } = false;
        public string? MotherName { get; set; } = string.Empty;
        public string? MotherMobileNo { get; set; } = string.Empty;
        public string? FatherName { get; set; } = string.Empty;
        public string? FatherMobileNo { get; set; } = string.Empty;
        public bool IsIndigenous { get; set; } = false;
        public bool IsSoloParent { get; set; } = false;
        public bool IsWithDisabilty { get; set; } = false;
        public bool Is4psMember { get; set; } = false;
        public int? ExamScore { get; set; }
        public int? AdmittedToProgramId { get; set; }
        public string? AdmittedToProgramName { get; set; }
        [Column("AdmittedToSelectedProgramTypeId")]
        public SelectedProgramTypes AdmittedToSelectedProgramType { get; set; }
        [Column("ApplicantTypeId")]
        public ApplicantTypes ApplicantType { get; set; }
        [Column("StatusId")]
        public ApplicantStatus Status { get; set; }
        public string? SchoolYear { get; set; } = string.Empty;
        public SemesterTypes Semester { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
    }
}
