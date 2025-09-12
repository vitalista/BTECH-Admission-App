using static BTECH_APP.Enums;

namespace BTECH_APP.Models
{
    public class UserAccess
    {
        public Dictionary<RoleTypes, string[]> RolePermissions { get; set; }

        public UserAccess()
        {
            RolePermissions = new Dictionary<RoleTypes, string[]>
            {
                { RoleTypes.Admin, new string[] {"/", "/dashboard", "/applicants", "/institutes", "/programs","/requirements", "/user-managements" , "/profile"} },
                { RoleTypes.Verifier, new string[] { "/", "/applicants", "/profile" } },
                { RoleTypes.Scheduler, new string[] { "/", "/applicants", "/profile" } },
                { RoleTypes.Applicant, new string[] { "/", "/btech-applicant" } }
            };
        }

        public bool HasAccess(RoleTypes role, string page) =>
             RolePermissions.ContainsKey(role) &&
             RolePermissions[role].Any(p => string.Equals(p, page, StringComparison.OrdinalIgnoreCase));

        public bool IsPageIsExist(string page) =>
             RolePermissions.Values.SelectMany(urlArray => urlArray).Distinct()
             .Any(p => string.Equals(p, page, StringComparison.OrdinalIgnoreCase));
    }
}
