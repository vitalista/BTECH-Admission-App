using BTECH_APP.Shared.Class;

namespace BTECH_APP.Models.Admin.Program
{
    public class ListProgramModel : ModifiedBy
    {
        public int ProgramId { get; set; }
        public string InstituteName { get; set; } = string.Empty;
        public string InstituteAcronym { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Acronym { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = string.Empty;
    }
}
