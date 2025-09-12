using BTECH_APP.Models.Guest;

namespace BTECH_APP.Services.Guest.Interfaces
{
    public interface IGuestService
    {
        Task<List<InstituteGuestModel>> Institutes();
        Task<RequirementGuestModel> Requirements();
    }
}
