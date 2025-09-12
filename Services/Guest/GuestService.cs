using BTECH_APP.Models;
using BTECH_APP.Models.Guest;
using BTECH_APP.Services.Guest.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BTECH_APP.Services.Guest
{
    public class GuestService : IGuestService
    {
        private readonly BTECHDbContext _dbContext;
        private readonly UserContext _userContext;

        public GuestService(BTECHDbContext dbContext, UserContext userContext)
        {
            _dbContext = dbContext;
            _userContext = userContext;
        }

        public async Task<List<InstituteGuestModel>> Institutes()
        {

            var query = await (from institute in _dbContext.Institutes.AsNoTracking()
                               where !institute.Deleted && institute.IsActive
                               orderby institute.Name
                               select new
                               {
                                   IntituteId = institute.InstituteId,
                                   Name = $"{institute.Name} ({institute.Acronym})",
                                   FilePath = institute.FilePath
                               }).ToListAsync();

            var instituteIds = query.Select(x => x.IntituteId).ToList();

            var programs = await _dbContext.Progams.AsNoTracking()
                           .Where(x => !x.Deleted && x.IsActive).ToListAsync();

            var result = query.Select(x => new InstituteGuestModel
            {
                Name = x.Name,
                Programs = programs.Where(y => y.InstituteId == x.IntituteId).Select(x => $"{x.Name} ({x.Acronym})").ToList(),
                FilePath = x.FilePath
            }).ToList();

            return result;
        }

        public async Task<RequirementGuestModel> Requirements()
        {
            var query = await (from requirement in _dbContext.Requirements.AsNoTracking()
                               where !requirement.Deleted && requirement.IsActive
                               select new
                               {
                                   requirement.Name,
                                   requirement.IsForTransferee,
                                   requirement.IsForFreshmen,
                                   requirement.IsForAlsGraduate
                               }).ToListAsync();

            RequirementGuestModel result = new()
            {
                Freshmen = query.Where(x => x.IsForFreshmen).Select(x => x.Name).ToList(),
                Transferee = query.Where(x => x.IsForTransferee).Select(x => x.Name).ToList(),
                AlsGraduate = query.Where(x => x.IsForAlsGraduate).Select(x => x.Name).ToList(),
            };

            return result;
        }
    }
}
