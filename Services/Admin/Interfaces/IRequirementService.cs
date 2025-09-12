using BTECH_APP.Models.Admin.Requirement;
using static BTECH_APP.Enums;

namespace BTECH_APP.Services.Admin.Interfaces
{
    public interface IRequirementService
    {
        Task<List<ListRequirementModel>> List();
        Task<List<LookupRequirementModel>> Lookup(ApplicantTypes applicantTypes);
        Task<SaveRequirementModel?> Find(int requirementId);
        Task<(bool success, string errorMessage)> Save(SaveRequirementModel model);
        Task Delete(int requirementId);
    }
}
