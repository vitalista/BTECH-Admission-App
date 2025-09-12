using static BTECH_APP.Enums;

namespace BTECH_APP.Models.Auth
{
    public class CurrentUserModel
    {
        public int UserId { get; set; }
        public int PersonId { get; set; }
        public int ApplicantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public ApplicantStatus ApplicantStatus { get; set; }
        public RoleTypes Role { get; set; }
        public DateTime ExpiresIn { get; set; }
    }
}
