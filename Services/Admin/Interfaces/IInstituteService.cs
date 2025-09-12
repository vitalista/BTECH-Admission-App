using BTECH_APP.Models.Admin.Institute;
using Microsoft.AspNetCore.Components.Forms;

namespace BTECH_APP.Services.Admin.Interfaces
{
    public interface IInstituteService
    {
        Task<List<ListInstituteModel>> List();
        Task<List<LookupInstituteModel>> Lookup();
        Task<SaveInstituteModel?> Find(int instituteId);
        Task<(bool success, string errorMessage)> Save(SaveInstituteModel model, IBrowserFile? file);
        Task Delete(int instituteId);
    }
}
