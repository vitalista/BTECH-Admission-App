using BTECH_APP.Shared.Class;
using static BTECH_APP.Enums;

public class ListUserManagementModel : ModifiedBy
{
    public int PersonId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Email { get; set; } = string.Empty;
    public RoleTypes Role { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
}
