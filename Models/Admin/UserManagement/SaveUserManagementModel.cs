using static BTECH_APP.Enums;

namespace BTECH_APP.Models.Admin.UserManagement
{
    public class SaveUserManagementModel
    {
        public int PersonId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Suffix { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public RoleTypes Role { get; set; } = RoleTypes.Admin;
        public bool IsActive { get; set; } = true;
    }
}
