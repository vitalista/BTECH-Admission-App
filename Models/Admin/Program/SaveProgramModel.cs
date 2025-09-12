namespace BTECH_APP.Models.Admin.Program
{
    public class SaveProgramModel
    {
        public int ProgramId { get; set; }
        public int InstituteId { get; set; }
        public string? Name { get; set; }
        public string? Acronym { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
