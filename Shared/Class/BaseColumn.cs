namespace BTECH_APP.Shared.Class
{
    public class BaseColumn
    {
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int ModifiedByUserId { get; set; }
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public bool Deleted { get; set; } = false;
        public long? DeletedByUserId { get; set; } = 0;
        public DateTime? DeletedDate { get; set; }
    }
}
