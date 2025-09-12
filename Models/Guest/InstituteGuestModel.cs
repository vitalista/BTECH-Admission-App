namespace BTECH_APP.Models.Guest
{
    public class InstituteGuestModel
    {
        public string? Name { get; set; }
        public List<string> Programs { get; set; } = new();
        public string FilePath { get; set; } = string.Empty;
    }

}
