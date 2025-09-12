using BTECH_APP.Entities.Admin.Dashboard;
using BTECH_APP.Helpers;
using BTECH_APP.Models.Admin.Dashboard;
using BTECH_APP.Services.Admin.Interfaces;
using Microsoft.EntityFrameworkCore;
using static BTECH_APP.Enums;

namespace BTECH_APP.Services.Admin
{
    public class AcademicYearService : IAcademicYearService
    {

        private readonly BTECHDbContext _dbContext;

        public AcademicYearService(BTECHDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SaveAcademicYearModel?> GetCurrentAcademic()
        {
            var currentSchoolYear = Helper.GetCurrentSchoolYear();

            var query = await (from entity in _dbContext.AcademicYears.AsNoTracking()
                               where entity.SchoolYear == currentSchoolYear

                               orderby entity.Semester descending

                               select new SaveAcademicYearModel
                               {
                                   AcademicId = entity.AcademicId,
                                   SchoolYear = entity.SchoolYear,
                                   Semester = entity.Semester,
                                   IsActive = entity.IsActive,
                               }).FirstOrDefaultAsync();

            if (query == null)
            {
                AcademicYearEntity entity = new()
                {
                    SchoolYear = currentSchoolYear,
                    Semester = SemesterTypes.FirstSemester,
                    IsActive = true,
                };

                await _dbContext.AcademicYears.AddAsync(entity);

                await _dbContext.SaveChangesAsync();

                query = new SaveAcademicYearModel
                {
                    AcademicId = entity.AcademicId,
                    SchoolYear = entity.SchoolYear,
                    Semester = entity.Semester,
                    IsActive = entity.IsActive,
                };
            }

            return query;
        }

        public async Task<bool> Toggle(SaveAcademicYearModel model)
        {
            var academicYear = await _dbContext.AcademicYears.FirstOrDefaultAsync(x => x.AcademicId == model.AcademicId);

            if (academicYear != null)
            {
                academicYear.IsActive = model.IsActive;
                academicYear.Semester = model.Semester;

                _dbContext.AcademicYears.Update(academicYear);
                await _dbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<string>> Lookup()
        {
            return await _dbContext.AcademicYears
                .AsNoTracking()
                .Select(x => x.SchoolYear)
                .ToListAsync();
        }

        public async Task<(string schoolYear, string semester, bool IsActive)> IsAcademicYearOpen()
        {
            var currentSchoolYear = Helper.GetCurrentSchoolYear();

            var query = await (from entity in _dbContext.AcademicYears.AsNoTracking()
                               where entity.SchoolYear == currentSchoolYear

                               orderby entity.Semester descending

                               select new SaveAcademicYearModel
                               {
                                   AcademicId = entity.AcademicId,
                                   SchoolYear = entity.SchoolYear,
                                   Semester = entity.Semester,
                                   IsActive = entity.IsActive,
                               }).FirstAsync();

            return (query.SchoolYear, query.Semester.GetDisplayName(), query.IsActive);
        }
    }
}
