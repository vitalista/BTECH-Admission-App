using Blazored.LocalStorage;
using BTECH_APP.Helpers;
using BTECH_APP.Models;
using BTECH_APP.Models.Admin.Program;
using BTECH_APP.Models.Auth;
using BTECH_APP.Services.Applicant.Interfaces;
using BTECH_APP.Services.Email;
using Microsoft.EntityFrameworkCore;
using static BTECH_APP.Enums;

namespace BTECH_APP.Services.Applicant
{
    public class Step5ApplicantService : IStep5ApplicantService
    {
        private readonly BTECHDbContext _dbContext;
        private readonly UserContext _userContext;
        private readonly IEmailService _emailService;
        private readonly ILocalStorageService _localStorage;

        public Step5ApplicantService(BTECHDbContext dbContext, UserContext userContext, IEmailService emailService, ILocalStorageService localStorageService)
        {
            _dbContext = dbContext;
            _userContext = userContext;
            _emailService = emailService;
            _localStorage = localStorageService;
        }

        public async Task<bool> Save(int programId)
        {
            int applicantId = _userContext.CurrentUser.ApplicantId;

            var applicant = await _dbContext.Applicants.FirstOrDefaultAsync(x => x.ApplicantId == applicantId);

            if (applicant == null)
                return false;

            applicant.Status = ApplicantStatus.Admitted;

            var selectedProgramInfo = await (
                       from selectedProgram in _dbContext.SelectedPrograms.AsNoTracking()
                       join program in _dbContext.Progams.AsNoTracking() on selectedProgram.ProgramId equals program.ProgramId
                       where selectedProgram.ProgramId == programId
                       && selectedProgram.ApplicantId == applicant.ApplicantId
                       select new
                       {
                           selectedProgram.ProgramId,
                           program.Name,
                           program.Acronym,
                           selectedProgram.SelectedProgramType,
                       }
                       ).FirstAsync();

            applicant.AdmittedToProgramId = selectedProgramInfo.ProgramId;
            applicant.AdmittedToProgramName = $"{selectedProgramInfo.Name} ({selectedProgramInfo.Acronym})";
            applicant.AdmittedToSelectedProgramType = selectedProgramInfo.SelectedProgramType;

            Helper.SetAuditFields(applicantId, applicant, _userContext.CurrentUser.UserId);

            await _dbContext.SaveChangesAsync();

            _userContext.CurrentUser = new CurrentUserModel
            {
                UserId = _userContext.CurrentUser.UserId,
                PersonId = _userContext.CurrentUser.PersonId,
                ApplicantId = applicant == null ? 0 : applicant.ApplicantId,
                Role = _userContext.CurrentUser.Role,
                Name = _userContext.CurrentUser.Name,
                ExpiresIn = _userContext.CurrentUser.ExpiresIn,
            };

            await _localStorage.SetItemAsync("LoggedInUser", _userContext.CurrentUser);

            var user = await _emailService.GetApplicantEmail(applicant.ApplicantId);

            if (!string.IsNullOrEmpty(user.email) && !string.IsNullOrEmpty(user.fullname))
            {
                _ = Task.Run(() => _emailService.SendEmail(
                      user.email,
                      user.fullname,
                      $"Application {applicant.Status.GetDisplayName()}",
                      $"You are officially admitted you into the {applicant.AdmittedToProgramName} program. Congratulations on reaching this important milestone."
                  ));
            }

            return true;
        }

        public async Task<(ApplicantStatus status, string programName, string reason)> Status()
        {
            int applicantId = _userContext.CurrentUser.ApplicantId;

            var applicant = await _dbContext.Applicants.FirstOrDefaultAsync(x => x.ApplicantId == applicantId);

            if (applicant == null)
                return (ApplicantStatus.Draft, string.Empty, string.Empty);

            var reason = await _dbContext.StatusRemarks.AsNoTracking()
                .Where(x => x.ApplicantId == applicantId && x.Status == applicant.Status)
                .Select(x => x.Remarks)
                .FirstOrDefaultAsync();

            return (applicant.Status, applicant.AdmittedToProgramName ?? string.Empty, reason ?? string.Empty);

        }

        public async Task<List<LookupProgramModel>> SuggestedProgram()
        {
            int applicantId = _userContext.CurrentUser.ApplicantId;
            var selectedProgramIds = await (_dbContext.SelectedPrograms.AsNoTracking()
                                    .Where(x => x.ApplicantId == applicantId && x.SelectedProgramType == SelectedProgramTypes.Recommended)
                                    .OrderBy(x => x.SelectedProgramType)
            .Select(x => x.ProgramId)).ToListAsync();

            var query = from program in _dbContext.Progams.AsNoTracking()
                        where selectedProgramIds.Contains(program.ProgramId)
                        select new LookupProgramModel
                        {
                            ProgramId = program.ProgramId,
                            Name = program.Name,
                        };

            return await query.ToListAsync();
        }
    }
}
