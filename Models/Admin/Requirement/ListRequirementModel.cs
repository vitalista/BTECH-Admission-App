using BTECH_APP.Shared.Class;

namespace BTECH_APP.Models.Admin.Requirement
{
    public class ListRequirementModel : ModifiedBy
    {
        public int RequirementId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsForFreshmen { get; set; } = true;
        public bool IsForTransferee { get; set; } = true;
        public bool IsForAlsGraduate { get; set; } = true;
        public bool IsRequired { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = string.Empty;
    }
}
