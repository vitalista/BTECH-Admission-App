using BTECH_APP.Models.Auth;

namespace BTECH_APP.Models
{
    public class UserContext
    {
        public CurrentUserModel CurrentUser { get; set; } = new();
    }
}
