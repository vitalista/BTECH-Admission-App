using BTECH_APP.Entities.Admin;
using BTECH_APP.Entities.Admin.Dashboard;
using BTECH_APP.Entities.Applicant;
using BTECH_APP.Entities.Auth;
using BTECH_APP.Entities.StaticData;
using Microsoft.EntityFrameworkCore;
using static BTECH_APP.Enums;

namespace BTECH_APP
{
    public class BTECHDbContext : DbContext
    {
        public BTECHDbContext(DbContextOptions<BTECHDbContext> options) : base(options) { }

        public DbSet<AddressEntity> Address { get; set; }
        public DbSet<ApplicantEntity> Applicants { get; set; }
        public DbSet<AcademicYearEntity> AcademicYears { get; set; }
        public DbSet<ApplicantRequirementEntity> ApplicantRequirements { get; set; }
        public DbSet<StatusRemarksEntity> StatusRemarks { get; set; }
        public DbSet<BarangayEntity> Barangays { get; set; }
        public DbSet<InstituteEntity> Institutes { get; set; }
        public DbSet<MunicipalityEntity> Municipalities { get; set; }
        public DbSet<PrevSchoolEntity> PrevSchools { get; set; }
        public DbSet<ProgamEntity> Progams { get; set; }
        public DbSet<ProvinceEntity> Provinces { get; set; }
        public DbSet<RequirementEntity> Requirements { get; set; }
        public DbSet<SelectedProgramEntity> SelectedPrograms { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<UserInformationEntity> UserInformations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Authentication

            modelBuilder.Entity<UserInformationEntity>().HasData(
                new UserInformationEntity
                {
                    PersonId = 1,
                    FirstName = "Juan Ponce",
                    MiddleName = "Marchal",
                    LastName = "Tech",
                    BirthDate = new DateTime(1999, 3, 7, 12, 0, 0, DateTimeKind.Utc),
                    MobileNo = "09673329712",
                    CreatedByUserId = 1,
                    CreatedDate = new DateTime(2024, 3, 7, 12, 0, 0, DateTimeKind.Utc),
                    ModifiedByUserId = 0,
                    ModifiedDate = new DateTime(2024, 3, 7, 12, 0, 0, DateTimeKind.Utc),
                    Deleted = false,
                    DeletedByUserId = 0
                });

            modelBuilder.Entity<UserEntity>().HasData(
                new UserEntity
                {
                    UserId = 1,
                    PersonId = 1,
                    Email = "admin@gmail.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("hello"),
                    IsDefaultPassword = false,
                    Role = RoleTypes.Admin,
                    CreatedByUserId = 1,
                    CreatedDate = new DateTime(2024, 3, 7, 12, 0, 0, DateTimeKind.Utc),
                    ModifiedByUserId = 0,
                    ModifiedDate = new DateTime(2024, 3, 7, 12, 0, 0, DateTimeKind.Utc),
                    Deleted = false,
                    DeletedByUserId = 0,
                    IsActive = true
                });

            #endregion Authentication
        }
    }
}
