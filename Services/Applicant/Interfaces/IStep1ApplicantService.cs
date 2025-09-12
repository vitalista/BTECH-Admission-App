using BTECH_APP.Models.Applicant;

namespace BTECH_APP.Services.Applicant.Interfaces
{
    public interface IStep1ApplicantService
    {
        Task<SaveApplicantModel> Find();
        Task<bool> SaveDraft(SaveApplicantModel model);
        Task<bool> Save(int applicantId);
    }
}
