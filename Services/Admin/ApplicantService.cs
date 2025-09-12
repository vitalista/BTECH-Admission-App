using BTECH_APP.Entities.Applicant;
using BTECH_APP.Helpers;
using BTECH_APP.Models;
using BTECH_APP.Models.Admin.Applicant;
using BTECH_APP.Models.Admin.Program;
using BTECH_APP.Models.Applicant;
using BTECH_APP.Services.Admin.Interfaces;
using BTECH_APP.Services.Email;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using static BTECH_APP.Enums;

namespace BTECH_APP.Services.Admin
{
    public class ApplicantService : IApplicantService
    {
        private readonly BTECHDbContext _dbContext;
        private readonly UserContext _userContext;
        private readonly IEmailService _emailService;

        public ApplicantService(BTECHDbContext dbContext, UserContext userContext, IEmailService emailService)
        {
            _dbContext = dbContext;
            _userContext = userContext;
            _emailService = emailService;
        }

        public async Task<List<ListApplicantModel>> List(List<string> schoolYears, List<SemesterTypes> semesters,
                                                    List<ApplicantTypes> applicantTypes, List<ApplicantStatus> applicantStatuses)
        {
            ApplicantStatus[] excludeStatus = { ApplicantStatus.Draft, ApplicantStatus.ForRequirements };

            var query = from entity in _dbContext.Applicants.AsNoTracking()
                        join userInformation in _dbContext.UserInformations.AsNoTracking() on entity.PersonId equals userInformation.PersonId
                        join modifiedByUser in _dbContext.Users.AsNoTracking() on entity.ModifiedByUserId equals modifiedByUser.UserId
                        join modifiedBy in _dbContext.UserInformations.AsNoTracking() on modifiedByUser.PersonId equals modifiedBy.PersonId
                        where !excludeStatus.Contains(entity.Status)
                        && schoolYears.Contains(entity.SchoolYear ?? Helper.GetCurrentSchoolYear())
                        && (semesters.Count < 1 || semesters.Contains(entity.Semester))
                        && (applicantTypes.Count < 1 || applicantTypes.Contains(entity.ApplicantType))
                        && (applicantStatuses.Count < 1 || applicantStatuses.Contains(entity.Status))
                        orderby entity.ModifiedDate descending

                        select new ListApplicantModel
                        {
                            ApplicantId = entity.ApplicantId,
                            ApplicantNo = entity.ApplicantNo ?? string.Empty,
                            SubmittedDate = entity.SubmittedDate ?? DateTime.Now,
                            Name = userInformation.FullName,
                            Birthday = userInformation.BirthDate,
                            Gender = userInformation.Gender.GetDisplayName(),
                            Programs = (from selectedProgram in _dbContext.SelectedPrograms.AsNoTracking()
                                        join program in _dbContext.Progams.AsNoTracking()
                                            on selectedProgram.ProgramId equals program.ProgramId
                                        where selectedProgram.ApplicantId == entity.ApplicantId
                                        && selectedProgram.SelectedProgramType != SelectedProgramTypes.Recommended
                                        orderby selectedProgram.SelectedProgramType
                                        select program.Name).ToList(),
                            ApplicantTypeName = entity.ApplicantType.GetDisplayName(),
                            StatusName = entity.Status.GetDisplayName(),
                            ModifiedDate = entity.ModifiedDate,
                            ModifiedBy = modifiedBy.FullName
                        };

            return await query.ToListAsync();
        }

        public async Task<List<LookupProgramModel>> LookupProgram(int applicantId, bool isSelected)
        {
            var selectedProgramIds = await (_dbContext.SelectedPrograms.AsNoTracking()
                                     .Where(x => x.ApplicantId == applicantId && x.SelectedProgramType != SelectedProgramTypes.Recommended)
                                     .OrderBy(x => x.SelectedProgramType)
                                     .Select(x => x.ProgramId)).ToListAsync();

            var query = from program in _dbContext.Progams.AsNoTracking()
                        where (isSelected && selectedProgramIds.Contains(program.ProgramId))
                            || (!isSelected && !selectedProgramIds.Contains(program.ProgramId))
                        select new LookupProgramModel
                        {
                            ProgramId = program.ProgramId,
                            Name = program.Name,
                        };

            return await query.ToListAsync();
        }

        public async Task<AdmisisionFormApplicantModel> FindAdmissionForm(int applicantId)
        {
            var applicant = await _dbContext.Applicants
                .AsNoTracking()
                .Where(a => a.ApplicantId == applicantId)
                .FirstAsync();

            var userInformation = await _dbContext.UserInformations.AsNoTracking()
                .Where(ui => !ui.Deleted && ui.PersonId == applicant.PersonId)
                .FirstAsync();

            var email = await _dbContext.Users.AsNoTracking()
            .Where(u => !u.Deleted && u.UserId == applicant.UserId)
            .Select(u => u.Email)
            .FirstAsync();

            var addressList = await (
                from address in _dbContext.Address.AsNoTracking()
                join province in _dbContext.Provinces.AsNoTracking() on address.ProvinceId equals province.ProvinceId
                join municipality in _dbContext.Municipalities.AsNoTracking() on address.MunicipalityId equals municipality.MunicipalityId
                join barangay in _dbContext.Barangays.AsNoTracking() on address.BarangayId equals barangay.BarangayId
                where address.ApplicantId == applicant.ApplicantId
                orderby address.AddressType ascending
                select new
                {
                    Address = $"{address.Street}, {barangay.Name}, {municipality.Name}, {province.Name} Zip Code: {address.ZipCode}",
                }).ToListAsync();


            var selectedPrograms = await (from selectedProgram in _dbContext.SelectedPrograms.AsNoTracking()
                                          join program in _dbContext.Progams.AsNoTracking() on selectedProgram.ProgramId equals program.ProgramId
                                          where selectedProgram.ApplicantId == applicant.ApplicantId
                                          orderby selectedProgram.SelectedProgramType ascending
                                          select new
                                          {
                                              ProgramName = program.Name
                                          }).ToListAsync();

            var prevSchools = await _dbContext.PrevSchools
                .AsNoTracking()
                .Where(ps => ps.ApplicantId == applicantId)
                .OrderBy(ps => ps.EducationType)
                .Select(ps => new SavePrevSchoolApplicantModel
                {
                    SchoolId = ps.SchoolId,
                    Name = ps.Name,
                    Address = ps.Address,
                    YearCompleted = ps.YearCompleted,
                    EducationType = ps.EducationType
                }).ToListAsync();

            var requirements = await Requirements(applicantId);

            return new AdmisisionFormApplicantModel
            {
                ApplicantId = applicantId,
                PersonId = applicant.PersonId,
                UserId = applicant.UserId,
                FirstName = userInformation.FirstName,
                MiddleName = userInformation.MiddleName,
                LastName = userInformation.LastName,
                Suffix = userInformation.Suffix,
                Email = email,
                BirthDate = Helper.FormatDate(userInformation.BirthDate),
                Age = Helper.CalculateAge(userInformation.BirthDate),
                ApplicantNo = applicant.ApplicantNo,
                IsSameAddress = applicant.IsSameAddress,
                MobileNo = userInformation.MobileNo,
                Gender = userInformation.Gender.GetDisplayName(),
                PlaceOfBirth = userInformation.PlaceOfBirth,
                CivilStatus = userInformation.CivilStatus.GetDisplayName(),
                NameOfSpouse = userInformation.NameOfSpouse,
                MotherName = applicant.MotherName,
                MotherMobileNo = applicant.MotherMobileNo,
                FatherName = applicant.FatherName,
                FatherMobileNo = applicant.FatherMobileNo,
                IsIndigenous = applicant.IsIndigenous ? "Yes" : "No",
                IsSoloParent = applicant.IsSoloParent ? "Yes" : "No",
                IsWithDisabilty = applicant.IsWithDisabilty ? "Yes" : "No",
                Is4psMember = applicant.Is4psMember ? "Yes" : "No",
                ApplicantType = applicant.ApplicantType.GetDisplayName(),
                PermenentAddress = addressList.Count > 0 ? addressList[0].Address : string.Empty,
                PresentAddress = addressList.Count > 1 ? addressList[1].Address : string.Empty,
                FirstChoice = selectedPrograms.Count > 0 ? selectedPrograms[0].ProgramName : string.Empty,
                SecondChoice = selectedPrograms.Count > 1 ? selectedPrograms[1].ProgramName : string.Empty,
                PrevSchools = prevSchools,
                Requirements = requirements
            };

        }

        public async Task<bool> SetStatus(int applicantId, string? remarks, ApplicantStatus status, DateTime? scheduleDate)
        {
            var applicant = await _dbContext.Applicants.FirstOrDefaultAsync(x => x.ApplicantId == applicantId);

            if (applicant == null)
                return false;

            applicant.Status = status;
            applicant.ScheduleDate = scheduleDate;

            string message = string.Empty;
            message = Helper.StatusEmailMessage(status, _userContext.CurrentUser.Name, remarks, applicant.ScheduleDate);

            StatusRemarksEntity remarksEntity = new()
            {
                ApplicantId = applicantId,
                Status = status,
                Description = message,
                Remarks = remarks,
                Date = DateTime.Now
            };

            await _dbContext.AddAsync(remarksEntity);

            await _dbContext.SaveChangesAsync();

            var user = await _emailService.GetApplicantEmail(applicantId);

            if (!string.IsNullOrEmpty(user.email) && !string.IsNullOrEmpty(user.fullname))
            {
                _ = Task.Run(() => _emailService.SendEmail(
                      user.email,
                      user.fullname,
                      $"Application {applicant.Status.GetDisplayName()}",
                      message
                  ));
            }

            return true;
        }

        public async Task<bool> Evaluate(int applicantId, string? remarks, ApplicantStatus status, int selectedProgramId, List<int> recommendedProgramIds)
        {
            var applicant = await _dbContext.Applicants.FirstOrDefaultAsync(x => x.ApplicantId == applicantId);

            if (applicant == null)
                return false;

            applicant.Status = status;

            string message = string.Empty;
            message = Helper.StatusEmailMessage(status, _userContext.CurrentUser.Name, remarks, applicant.ScheduleDate);

            StatusRemarksEntity remarksEntity = new()
            {
                ApplicantId = applicantId,
                Status = status,
                Description = message,
                Remarks = remarks,
                Date = DateTime.Now
            };

            if (status == ApplicantStatus.Recommending)
            {
                List<SelectedProgramEntity> selectedProgramEntities = new List<SelectedProgramEntity>();

                if (recommendedProgramIds.Any())
                {
                    foreach (var item in recommendedProgramIds)
                    {
                        SelectedProgramEntity selectedProgramEntity = new()
                        {
                            ApplicantId = applicant.ApplicantId,
                            ProgramId = item,
                            SelectedProgramType = SelectedProgramTypes.Recommended

                        };

                        selectedProgramEntities.Add(selectedProgramEntity);
                    }
                }

                await _dbContext.AddRangeAsync(selectedProgramEntities);
            }
            else if (status == ApplicantStatus.Admitted)
            {
                if (selectedProgramId != 0)
                {
                    var selectedProgramInfo = await (
                        from selectedProgram in _dbContext.SelectedPrograms.AsNoTracking()
                        join program in _dbContext.Progams.AsNoTracking() on selectedProgram.ProgramId equals program.ProgramId
                        where selectedProgram.ProgramId == selectedProgramId
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
                }
            }

            Helper.SetAuditFields(applicantId, applicant, _userContext.CurrentUser.UserId);

            await _dbContext.AddAsync(remarksEntity);

            await _dbContext.SaveChangesAsync();

            var user = await _emailService.GetApplicantEmail(applicantId);

            if (!string.IsNullOrEmpty(user.email) && !string.IsNullOrEmpty(user.fullname))
            {
                _ = Task.Run(() => _emailService.SendEmail(
                      user.email,
                      user.fullname,
                      $"Application {applicant.Status.GetDisplayName()}",
                      (status == ApplicantStatus.Admitted ?
                      $"<b>{_userContext.CurrentUser.Name}</b> has officially admitted you into the {applicant.AdmittedToProgramName} program. Congratulations on reaching this important milestone." :
                      message
                      )
                  ));
            }

            return true;
        }

        public string ExcelData(List<ListApplicantModel> data)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Applicants");

            // Title Row
            worksheet.Cell("A1").Value = "Baliwag Polytechnic College";
            worksheet.Range("A1:K1").Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(16)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.DarkGreen)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Subtitle
            worksheet.Cell("A2").Value = "Applicants Masterlist";
            worksheet.Range("A2:K2").Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(12)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            // Date
            worksheet.Cell("A3").Value = DateTime.Now.ToString("MMM dd yyyy");
            worksheet.Range("A3:K3").Merge().Style
                .Font.SetFontSize(11)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            // Header Row
            worksheet.Cell(5, 1).Value = "Applicant No.";
            worksheet.Cell(5, 2).Value = "Name";
            worksheet.Cell(5, 3).Value = "Birthday";
            worksheet.Cell(5, 4).Value = "Age";
            worksheet.Cell(5, 5).Value = "Gender";
            worksheet.Cell(5, 6).Value = "Selected Programs";
            worksheet.Cell(5, 7).Value = "Type";
            worksheet.Cell(5, 8).Value = "Status";
            worksheet.Cell(5, 9).Value = "Submitted Date";
            worksheet.Cell(5, 10).Value = "Modified Date";
            worksheet.Cell(5, 11).Value = "Modified By";

            // Header Styling
            worksheet.Range("A5:K5").Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.LightBlue)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            worksheet.Range("A5:K5").SetAutoFilter();

            // Fill Data
            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 6;
                var item = data[i];

                worksheet.Cell(row, 1).Value = item.ApplicantNo;
                worksheet.Cell(row, 2).Value = item.Name;
                worksheet.Cell(row, 3).Value = Helper.FormatDate(item.Birthday);
                worksheet.Cell(row, 4).Value = item.Age;
                worksheet.Cell(row, 5).Value = item.Gender;
                worksheet.Cell(row, 6).Value = string.Join(", ", item.Programs ?? new List<string>());
                worksheet.Cell(row, 7).Value = item.ApplicantTypeName;
                worksheet.Cell(row, 8).Value = item.StatusName;
                worksheet.Cell(row, 9).Value = Helper.FormatDate(item.SubmittedDate);
                worksheet.Cell(row, 10).Value = Helper.FormatDate(item.ModifiedDate);
                worksheet.Cell(row, 11).Value = item.ModifiedBy;

                // Enable line wrap for long program lists
                worksheet.Cell(row, 6).Style.Alignment.WrapText = true;
            }

            int lastRow = data.Count + 5;

            // Apply borders only to filled data area
            worksheet.Range($"A5:K{lastRow}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range($"A5:K{lastRow}").Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Adjust columns to fit content
            worksheet.Columns().AdjustToContents();

            // Export to Base64
            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            var bytes = ms.ToArray();
            return Convert.ToBase64String(bytes);
        }

        private async Task<List<SaveRequirementsApplicantModel>> Requirements(int applicantId)
        {
            var applicant = await _dbContext.Applicants
                      .AsNoTracking()
                      .Where(a => a.ApplicantId == applicantId)
                      .FirstOrDefaultAsync();

            if (applicant == null)
                return new List<SaveRequirementsApplicantModel>();

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

            return finalResult;
        }

        public async Task<List<ActivityLogApplicantModel>> ActivityLogs(int applicantId)
        {
            return await (from remarks in _dbContext.StatusRemarks.AsNoTracking()
                          where remarks.ApplicantId == applicantId
                          orderby remarks.Date descending
                          select new ActivityLogApplicantModel
                          {
                              Description = remarks.Description,
                              Remarks = remarks.Remarks,
                              Date = remarks.Date ?? DateTime.Now,
                          }).ToListAsync();
        }
    }
}
