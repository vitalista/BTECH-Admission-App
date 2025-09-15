using BTECH_APP.Shared.Class;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static BTECH_APP.Enums;

namespace BTECH_APP.Entities.Auth
{


    [Table("users")]
    public class UserEntity : BaseColumn
    {
        [Key]
        public int UserId { get; set; }
        public int PersonId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsDefaultPassword { get; set; } = true;
        [Column("RoleId")]
        public RoleTypes Role { get; set; }
        public bool IsActive { get; set; }
    }
}
