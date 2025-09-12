using BTECH_APP.Models.Auth;

namespace BTECH_APP.Services.Auth.Interfaces
{
    public interface IAuthService
    {
        Task GetCurrentUser();
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        Task<bool> ValidateAccount(LoginModel model);
        Task<bool> LoginAsync(LoginModel model);
        Task<bool> Relogin(int userId);
        Task LogoutAsync();
        Task<(bool, string, RegisterModel)> RegisterAsync(RegisterModel model);
    }
}
