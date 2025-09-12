namespace BTECH_APP.Models.Guest
{
    public class RequirementGuestModel
    {
        public List<string> Freshmen { get; set; } = new();
        public List<string> Transferee { get; set; } = new();
        public List<string> AlsGraduate { get; set; } = new();
    }
}
