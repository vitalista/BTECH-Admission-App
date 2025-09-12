using BTECH_APP.Models.Admin.Program;
using static BTECH_APP.Enums;

namespace BTECH_APP.Services.Applicant.Interfaces
{
    public interface IStep5ApplicantService
    {
        Task<bool> Save(int programId);
        Task<List<LookupProgramModel>> SuggestedProgram();
        Task<(ApplicantStatus status, string programName, string reason)> Status();
    }
}
