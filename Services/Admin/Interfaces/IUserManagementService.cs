using BTECH_APP.Models.Admin.UserManagement;

namespace BTECH_APP.Services.Admin.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<ListUserManagementModel>> List();
        Task<SaveUserManagementModel?> Find(int userId);
        Task<(bool success, string errorMessage)> Save(SaveUserManagementModel model);
        Task Delete(int userId);
        Task<bool> ResetPassword(int userId);
        Task<bool> ChangePassword(SaveChangePasswordUserManagementModel model);

    }
}
