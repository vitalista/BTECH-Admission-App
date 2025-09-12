using AutoMapper;
using BTECH_APP.Entities.Applicant;
using BTECH_APP.Helpers;
using BTECH_APP.Models;
using BTECH_APP.Models.Applicant;
using BTECH_APP.Models.Auth;
using BTECH_APP.Services.Applicant.Interfaces;
using BTECH_APP.Services.Email;
using Microsoft.EntityFrameworkCore;
using static BTECH_APP.Enums;

namespace BTECH_APP.Services.Applicant
{
    public class Step2ApplicantService : IStep2ApplicantService
    {
        private readonly BTECHDbContext _dbContext;
        private readonly UserContext _userContext;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;


        public Step2ApplicantService(BTECHDbContext dbContext, UserContext userContext, IMapper mapper, IEmailService emailService)
        {
            _dbContext = dbContext;
            _userContext = userContext;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<(List<SaveRequirementsApplicantModel> requirements, string? reason)> List()
        {
            var applicant = await _dbContext.Applicants
                      .AsNoTracking()
                      .Where(a => a.PersonId == _userContext.CurrentUser.PersonId)
                      .FirstOrDefaultAsync();

            if (applicant == null)
                return (new List<SaveRequirementsApplicantModel>(), string.Empty);

            var requirementsQuery = from requirement in _dbContext.Requirements.AsNoTracking()

                                    join applicantRequirement in _dbContext.ApplicantRequirements.AsNoTracking()
                                    on new { requirement.RequirementId, applicant.ApplicantId }
                                    equals new { applicantRequirement.RequirementId, applicantRequirement.ApplicantId }
                                    into applicantRequirementGroup
                                    from applicantRequirement in applicantRequirementGroup.DefaultIfEmpty()

                                    orderby requirement.Name
                                    where !requirement.Deleted && requirement.IsActive &&
                                          (
                                              (applicant.ApplicantType == ApplicantTypes.Freshmen && requirement.IsForFreshmen) ||
                                              (applicant.ApplicantType == ApplicantTypes.Transferee && requirement.IsForTransferee) ||
                                              (applicant.ApplicantType == ApplicantTypes.AlsGraduate && requirement.IsForAlsGraduate)
                                          )
                                    select new { requirement, applicantRequirement };

            var resultList = await requirementsQuery.ToListAsync();

            var finalResult = resultList.Select(x => new SaveRequirementsApplicantModel
            {
                ApplicantRequirementId = x.applicantRequirement?.ApplicantRequirementId ?? 0,
                RequirementId = x.requirement.RequirementId,
                Name = x.requirement.Name,
                FileName = x.applicantRequirement?.FileName,
                FilePath = x.applicantRequirement?.FilePath,
                FileType = x.applicantRequirement?.FileType ?? FileTypes.Invalid,
                IsRequired = x.requirement.IsRequired,
            }).ToList();

            var returnedReason = string.Empty;

            if (applicant.Status == ApplicantStatus.Returned)
            {
                returnedReason = await _dbContext.StatusRemarks.AsNoTracking()
                                .Where(x => x.ApplicantId == applicant.ApplicantId && x.Status == applicant.Status)
                                .Select(x => x.Remarks).FirstOrDefaultAsync();
            }

            return (finalResult, returnedReason);
        }

        public async Task<bool> Save(SaveRequirementsApplicantModel model)
        {
            ApplicantRequirementEntity entity = _mapper.Map<ApplicantRequirementEntity>(model);

            var applicant = await _dbContext.Applicants
                      .AsNoTracking()
                      .Where(a => a.PersonId == _userContext.CurrentUser.PersonId)
                      .FirstOrDefaultAsync();

            if (applicant == null)
                entity.ApplicantId = 0;
            else
                entity.ApplicantId = applicant.ApplicantId;

            if (entity.ApplicantRequirementId != 0)
            {
                var original = await _dbContext.ApplicantRequirements.FirstOrDefaultAsync(x => x.ApplicantRequirementId == entity.ApplicantRequirementId);

                if (original == null)
                    return false;

                entity = _mapper.Map(model, original);
            }

            if (entity.ApplicantRequirementId == 0)
                await _dbContext.ApplicantRequirements.AddAsync(entity);
            else
                _dbContext.ApplicantRequirements.Update(entity);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Submit(int applicantId)
        {
            var applicant = await _dbContext.Applicants
                     .Where(a => a.ApplicantId == applicantId)
                     .FirstOrDefaultAsync();

            if (applicant != null)
            {
                if (applicant.Status == ApplicantStatus.Returned)
                    applicant.Status = ApplicantStatus.Resubmitted;
                else if (applicant.Status == ApplicantStatus.ForRequirements)
                    applicant.Status = ApplicantStatus.Submitted;

                applicant.SubmittedDate = DateTime.UtcNow;

                ApplicantStatus[] excludeStatus = { ApplicantStatus.Draft, ApplicantStatus.ForRequirements };

                int nextSeqNo = 1;

                var currentSeq = await _dbContext.Applicants.AsNoTracking()
                                   .Where(a => !excludeStatus.Contains(a.Status))
                                   .Select(a => a.ApplicantNo).FirstOrDefaultAsync();

                if (currentSeq != null)
                    nextSeqNo = int.Parse(currentSeq.Split("-")[2]) + 1;

                applicant.ApplicantNo = Helper.GenerateApplicantNo(applicant.Semester, nextSeqNo);


                _dbContext.Update(applicant);

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

                var user = await _emailService.GetApplicantEmail(applicantId);

                if (!string.IsNullOrEmpty(user.email) && !string.IsNullOrEmpty(user.fullname))
                {
                    _ = Task.Run(() => _emailService.SendEmail(
                          user.email,
                          user.fullname,
                          $"Application {applicant.Status.GetDisplayName()}",
                          $"We are pleased to inform you that your application has been successfully {applicant.Status.GetDisplayName().ToLower()}.<br/>" +
                          $"Please wait until {Helper.FormatDate(DateTime.Now.AddDays(15))}, for you application review."
                      ));
                }
            }
            else
                return false;

            return true;
        }
    }
}
