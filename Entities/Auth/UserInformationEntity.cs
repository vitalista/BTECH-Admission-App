using BTECH_APP.Shared.Class;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static BTECH_APP.Enums;

namespace BTECH_APP.Entities.Auth
{

    [Table("Persons")]
    public class UserInformationEntity : BaseColumn
    {
        [Key]
        public int PersonId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Suffix { get; set; } = string.Empty;
        public string FullName => string.Join(" ", new[]
        {
            FirstName,
            string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Substring(0, 1) + ". ",
            LastName,
            Suffix
        }
        .Where(s => !string.IsNullOrWhiteSpace(s)))
        ;
        public DateTime BirthDate { get; set; }
        public string? MobileNo { get; set; } = string.Empty;
        [Column("GenderTypeId")]
        public GenderTypes Gender { get; set; }
        [Column("CivilStatusId")]
        public CivilStatus CivilStatus { get; set; }
        public string? NameOfSpouse { get; set; } = string.Empty;
        public string? PlaceOfBirth { get; set; } = string.Empty;
    }
}
