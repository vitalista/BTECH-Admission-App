using BTECH_APP.Models.Applicant;

namespace BTECH_APP.Services.Applicant.Interfaces
{
    public interface IStep2ApplicantService
    {
        Task<(List<SaveRequirementsApplicantModel> requirements, string? reason)> List();
        Task<bool> Save(SaveRequirementsApplicantModel model);
        Task<bool> Submit(int applicantId);
    }
}
