using BTECH_APP.Models.Admin.Applicant;
using BTECH_APP.Models.Admin.Program;
using static BTECH_APP.Enums;

namespace BTECH_APP.Services.Admin.Interfaces
{
    public interface IApplicantService
    {
        Task<List<ListApplicantModel>> List(List<string> schoolYears, List<SemesterTypes> semesters,
                                                    List<ApplicantTypes> applicantTypes, List<ApplicantStatus> applicantStatuses);
        Task<List<LookupProgramModel>> LookupProgram(int applicantId, bool isSelected);
        Task<AdmisisionFormApplicantModel> FindAdmissionForm(int applicantId);
        Task<bool> SetStatus(int applicantId, string? remarks, ApplicantStatus status, DateTime? scheduleDate);
        string ExcelData(List<ListApplicantModel> data);
        Task<bool> Evaluate(int applicantId, string? remarks, ApplicantStatus status, int selectedProgramId, List<int> recommendedProgramIds);
        Task<List<ActivityLogApplicantModel>> ActivityLogs(int applicantId);
    }
}
