using BTECH_APP.Models;
using BTECH_APP.Models.Auth;
using BTECH_APP.Services.Applicant.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BTECH_APP.Services.Applicant
{
    public class ApplicantService : IApplicantService
    {
        private readonly BTECHDbContext _dbContext;
        private readonly UserContext _userContext;

        public ApplicantService(BTECHDbContext dbContext, UserContext userContext)
        {
            _dbContext = dbContext;
            _userContext = userContext;
        }
        public Enums.ApplicantStatus ApplicantStatus()
        {
            CurrentUserModel currentUser = _userContext.CurrentUser;

            var applicant = _dbContext.Applicants.AsNoTracking().
                            Where(x => x.UserId == currentUser.UserId && x.PersonId == currentUser.PersonId)
                            .Select(x => x).FirstOrDefault();

            if (applicant == null)
                return Enums.ApplicantStatus.Draft;
            else
                return applicant.Status;
        }
    }
}
