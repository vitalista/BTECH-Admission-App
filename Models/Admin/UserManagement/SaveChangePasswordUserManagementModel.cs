namespace BTECH_APP.Models.Admin.UserManagement
{
    public class SaveChangePasswordUserManagementModel
    {
        public int UserId { get; set; }
        public string OldPassword { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
