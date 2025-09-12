namespace BTECH_APP.Shared.Class
{
    public class ModifiedBy
    {
        public int ModifiedByUserId { get; set; }
        public int ModifiedByPersonId { get; set; }
        public string ModifiedByName { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
    }
}
