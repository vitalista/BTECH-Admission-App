using static BTECH_APP.Enums;

namespace BTECH_APP.Models.Applicant
{
    public class SaveSelectedProgApplicantModel
    {
        public int SelectedProgramId { get; set; }
        public int ProgramId { get; set; }
        public SelectedProgramTypes SelectedProgramType { get; set; }
    }
}
