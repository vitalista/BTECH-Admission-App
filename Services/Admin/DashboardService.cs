using AutoMapper;
using BTECH_APP.Helpers;
using BTECH_APP.Models.Admin.Dashboard;
using BTECH_APP.Services.Admin.Interfaces;
using Microsoft.EntityFrameworkCore;
using static BTECH_APP.Enums;

namespace BTECH_APP.Services.Admin
{
    public class DashboardService : IDashboardService
    {
        private readonly BTECHDbContext _dbContext;
        private readonly IMapper _mapper;

        public DashboardService(BTECHDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<StatisticsModel> Statistic()
        {
            var currentSchoolYear = Helper.GetCurrentSchoolYear();

            ApplicantStatus[] excludeStatus = { ApplicantStatus.Draft, ApplicantStatus.ForRequirements };

            var applicants = await _dbContext.Applicants
                .AsNoTracking()
                .Where(applicant => applicant.SchoolYear == currentSchoolYear &&
                                    !excludeStatus.Contains(applicant.Status))
                .ToListAsync();

            var typeCounts = applicants
                .GroupBy(a => a.ApplicantType)
                .ToDictionary(g => g.Key, g => g.Count());

            var statusCounts = applicants
                .GroupBy(a => a.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var model = new StatisticsModel
            {
                TotalFreshmen = typeCounts.TryGetValue(ApplicantTypes.Freshmen, out var freshmen) ? freshmen : 0,
                TotalTransferee = typeCounts.TryGetValue(ApplicantTypes.Transferee, out var transferee) ? transferee : 0,
                TotalAlsGrad = typeCounts.TryGetValue(ApplicantTypes.AlsGraduate, out var alsGrad) ? alsGrad : 0,

                TotalSubmitted = statusCounts.TryGetValue(ApplicantStatus.Submitted, out var submitted) ? submitted : 0,
                TotalScheduled = statusCounts.TryGetValue(ApplicantStatus.Scheduled, out var scheduled) ? scheduled : 0,
                TotalRecommending = statusCounts.TryGetValue(ApplicantStatus.Recommending, out var recommending) ? recommending : 0,
                TotalAdmitted = statusCounts.TryGetValue(ApplicantStatus.Admitted, out var admitted) ? admitted : 0,
                TotalRejected = statusCounts.TryGetValue(ApplicantStatus.Rejected, out var rejected) ? rejected : 0
            };

            return model;
        }
    }
}
