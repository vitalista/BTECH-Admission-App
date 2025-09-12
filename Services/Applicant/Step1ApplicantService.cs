using AutoMapper;
using Blazored.LocalStorage;
using BTECH_APP.Entities.Applicant;
using BTECH_APP.Entities.Auth;
using BTECH_APP.Helpers;
using BTECH_APP.Models;
using BTECH_APP.Models.Applicant;
using BTECH_APP.Models.Auth;
using BTECH_APP.Models.StaticData;
using BTECH_APP.Services.Applicant.Interfaces;
using Microsoft.EntityFrameworkCore;
using static BTECH_APP.Enums;

namespace BTECH_APP.Services.Applicant
{
    public class Step1ApplicantService : IStep1ApplicantService
    {
        private readonly BTECHDbContext _dbContext;
        private readonly UserContext _userContext;
        private readonly IMapper _mapper;
        private readonly ILocalStorageService _localStorage;

        public Step1ApplicantService(BTECHDbContext dbContext, UserContext userContext, IMapper mapper, ILocalStorageService localStorageService)
        {
            _dbContext = dbContext;
            _userContext = userContext;
            _mapper = mapper;
            _localStorage = localStorageService;
        }

        public async Task<SaveApplicantModel> Find()
        {
            var userInformation = await _dbContext.UserInformations.AsNoTracking()
                .Where(ui => !ui.Deleted && ui.PersonId == _userContext.CurrentUser.PersonId)
                .FirstOrDefaultAsync();

            if (userInformation == null)
                userInformation = new UserInformationEntity();

            var applicant = await _dbContext.Applicants
                .AsNoTracking()
                .Where(a => !a.Deleted && a.UserId == _userContext.CurrentUser.UserId && a.Status == ApplicantStatus.Draft)
                .FirstOrDefaultAsync();

            var applicantId = 0;

            if (applicant != null)
                applicantId = applicant.ApplicantId;
            else
                applicant = new ApplicantEntity();

            var addressList = await _dbContext.Address
                .AsNoTracking()
                .Where(a => a.ApplicantId == applicantId)
                .OrderBy(a => a.AddressType)
                .Select(a => new SaveAddressApplicantModel
                {
                    AddressId = a.AddressId,
                    ProvinceId = a.ProvinceId,
                    MunicipalityId = a.MunicipalityId,
                    BarangayId = a.BarangayId,
                    Street = a.Street,
                    ZipCode = a.ZipCode,
                    AddressType = a.AddressType
                }).ToListAsync();

            var selectedPrograms = await _dbContext.SelectedPrograms
                .AsNoTracking()
                .Where(sp => sp.ApplicantId == applicantId)
                .OrderBy(sp => sp.SelectedProgramType)
                .Select(sp => new SaveSelectedProgApplicantModel
                {
                    SelectedProgramId = sp.SelectedProgramId,
                    ProgramId = sp.ProgramId,
                    SelectedProgramType = sp.SelectedProgramType
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

            var provinces = await _dbContext.Provinces
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new SaveProvinceModel
                {
                    ProvinceId = x.ProvinceId,
                    Name = x.Name,
                }).ToListAsync();

            var model = new SaveApplicantModel
            {
                ApplicantId = applicantId,
                PersonId = _userContext.CurrentUser.PersonId,
                UserId = _userContext.CurrentUser.UserId,
                IsSameAddress = applicant.IsSameAddress,
                MobileNo = userInformation.MobileNo,
                Gender = userInformation.Gender,
                PlaceOfBirth = userInformation.PlaceOfBirth,
                CivilStatus = userInformation.CivilStatus,
                NameOfSpouse = userInformation.NameOfSpouse,
                MotherName = applicant.MotherName,
                MotherMobileNo = applicant.MotherMobileNo,
                FatherName = applicant.FatherName,
                FatherMobileNo = applicant.FatherMobileNo,
                IsIndigenous = applicant.IsIndigenous,
                IsSoloParent = applicant.IsSoloParent,
                IsWithDisabilty = applicant.IsWithDisabilty,
                Is4psMember = applicant.Is4psMember,
                ApplicantType = applicant.ApplicantType,
                Address = addressList,
                SelectedPrograms = selectedPrograms,
                PrevSchools = prevSchools,

                PermanetProvinces = provinces,

                PermanetMunicipalities = _dbContext.Municipalities
                                         .AsNoTracking()
                                         .Where(x => x.ProvinceId == (addressList != null && addressList.Count > 0 ? addressList[0].ProvinceId : 0))
                                         .Select(x => new SaveMunicipalityModel
                                         {
                                             MunicipalityId = x.MunicipalityId,
                                             Name = x.Name,
                                         }).ToList(),

                PermanentBarangays = _dbContext.Barangays
                                    .AsNoTracking()
                                    .Where(x => x.MunicipalityId == (addressList != null && addressList.Count > 0 ? addressList[0].MunicipalityId : 0))
                                    .Select(x => new SaveBarangayModel
                                    {
                                        BarangayId = x.BarangayId,
                                        MunicipalityId = x.MunicipalityId,
                                        Name = x.Name,
                                    }).ToList(),

                CurrentProvinces = provinces,

                CurrentMunicipalities = _dbContext.Municipalities
                                         .AsNoTracking()
                                         .Where(x => x.ProvinceId == (addressList != null && addressList.Count > 1 ? addressList[1].ProvinceId : 0))
                                         .Select(x => new SaveMunicipalityModel
                                         {
                                             MunicipalityId = x.MunicipalityId,
                                             Name = x.Name,
                                         }).ToList(),

                CurrentBarangays = _dbContext.Barangays
                                    .AsNoTracking()
                                    .Where(x => x.MunicipalityId == (addressList != null && addressList.Count > 1 ? addressList[1].MunicipalityId : 0))
                                    .Select(x => new SaveBarangayModel
                                    {
                                        BarangayId = x.BarangayId,
                                        MunicipalityId = x.MunicipalityId,
                                        Name = x.Name,
                                    }).ToList(),
            };

            return model;
        }

        public async Task<bool> SaveDraft(SaveApplicantModel model)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                #region Academic Year
                var academicYear = await _dbContext.AcademicYears
                                  .AsNoTracking()
                                  .OrderByDescending(x => x.AcademicId)
                                  .FirstAsync();
                #endregion Academic Year
                #region User Information

                UserInformationEntity userInformation = _mapper.Map<UserInformationEntity>(model);

                if (userInformation.PersonId != 0)
                {
                    var originalUserInformation = await _dbContext.UserInformations.FirstOrDefaultAsync(x => x.PersonId == _userContext.CurrentUser.PersonId);

                    if (originalUserInformation != null)
                        userInformation = _mapper.Map(model, originalUserInformation);
                }

                Helper.SetAuditFields(userInformation.PersonId, userInformation, _userContext.CurrentUser.UserId);

                if (userInformation.PersonId == 0)
                    await _dbContext.UserInformations.AddAsync(userInformation);
                else
                    _dbContext.UserInformations.Update(userInformation);

                await _dbContext.SaveChangesAsync();

                #endregion User Information

                var applicantId = 0;

                if (_userContext.CurrentUser.ApplicantId == 0)
                {
                    var applicantExist = await _dbContext.Applicants.FirstOrDefaultAsync(x => x.PersonId == userInformation.PersonId);

                    if (applicantExist != null)
                    {
                        applicantId = applicantExist.ApplicantId;
                    }
                }
                else
                    applicantId = _userContext.CurrentUser.ApplicantId;

                model.ApplicantId = applicantId;


                #region Applicant

                ApplicantEntity applicant = _mapper.Map<ApplicantEntity>(model);

                if (applicant.ApplicantId != 0)
                {
                    var originalApplicant = await _dbContext.Applicants.FirstOrDefaultAsync(x => x.ApplicantId == applicant.ApplicantId);

                    if (originalApplicant != null)
                        applicant = _mapper.Map(model, originalApplicant);
                }

                applicant.Status = ApplicantStatus.Draft;
                applicant.PersonId = _userContext.CurrentUser.PersonId;
                applicant.UserId = _userContext.CurrentUser.UserId;
                applicant.SchoolYear = academicYear.SchoolYear;
                applicant.Semester = academicYear.Semester;

                Helper.SetAuditFields(applicant.ApplicantId, applicant, _userContext.CurrentUser.UserId);

                if (applicant.ApplicantId == 0)
                    await _dbContext.Applicants.AddAsync(applicant);
                else
                    _dbContext.Applicants.Update(applicant);

                await _dbContext.SaveChangesAsync();

                #endregion Applicant

                #region Address

                if (model.Address != null && model.Address.Any())
                {
                    foreach (var addressModel in model.Address)
                    {
                        AddressEntity address = _mapper.Map<AddressEntity>(addressModel);

                        if (address.AddressId != 0)
                        {
                            var originalAddress = await _dbContext.Address.FirstOrDefaultAsync(x => x.AddressId == address.AddressId);

                            if (originalAddress == null)
                                return false;

                            if (addressModel.ProvinceId != originalAddress.ProvinceId)
                            {
                                addressModel.MunicipalityId = 0;
                                addressModel.BarangayId = 0;
                            }

                            if (addressModel.MunicipalityId != originalAddress.MunicipalityId)
                                addressModel.BarangayId = 0;

                            address = _mapper.Map(addressModel, originalAddress);


                        }

                        address.ApplicantId = applicant.ApplicantId;

                        if (address.AddressId == 0)
                            await _dbContext.Address.AddAsync(address);
                        else
                            _dbContext.Address.Update(address);

                        await _dbContext.SaveChangesAsync();

                    }
                }
                #endregion Address

                #region Selected Program

                if (model.SelectedPrograms != null && model.SelectedPrograms.Any())
                {
                    foreach (var selectedProgramModel in model.SelectedPrograms)
                    {
                        SelectedProgramEntity selectedProgram = _mapper.Map<SelectedProgramEntity>(selectedProgramModel);

                        if (selectedProgram.SelectedProgramId != 0)
                        {
                            var originalSelectedProgram = await _dbContext.SelectedPrograms.FirstOrDefaultAsync(x => x.SelectedProgramId == selectedProgram.SelectedProgramId);

                            if (originalSelectedProgram == null)
                                return false;

                            selectedProgram = _mapper.Map(selectedProgramModel, originalSelectedProgram);
                        }

                        selectedProgram.ApplicantId = applicant.ApplicantId;

                        if (selectedProgram.SelectedProgramId == 0)
                            await _dbContext.SelectedPrograms.AddAsync(selectedProgram);
                        else
                            _dbContext.SelectedPrograms.Update(selectedProgram);

                        await _dbContext.SaveChangesAsync();

                    }
                }
                #endregion  Selected Program

                #region Prev School

                if (model.PrevSchools != null && model.PrevSchools.Any())
                {
                    foreach (var prevSchoolModel in model.PrevSchools)
                    {
                        PrevSchoolEntity prevSchool = _mapper.Map<PrevSchoolEntity>(prevSchoolModel);

                        if (prevSchool.SchoolId != 0)
                        {
                            var originalPrevSchool = await _dbContext.PrevSchools.FirstOrDefaultAsync(x => x.SchoolId == prevSchool.SchoolId);

                            if (originalPrevSchool == null)
                                return false;

                            prevSchool = _mapper.Map(prevSchoolModel, originalPrevSchool);
                        }

                        prevSchool.ApplicantId = applicant.ApplicantId;

                        if (prevSchool.SchoolId == 0)
                            await _dbContext.PrevSchools.AddAsync(prevSchool);
                        else
                            _dbContext.PrevSchools.Update(prevSchool);

                        await _dbContext.SaveChangesAsync();
                    }
                }
                #endregion  Prev School

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
            }

            return true;
        }

        public async Task<bool> Save(int applicantId)
        {
            #region Applicant

            var applicant = await _dbContext.Applicants.FirstOrDefaultAsync(x => x.ApplicantId == applicantId);

            if (applicant != null)
            {
                applicant.Status = ApplicantStatus.ForRequirements;

                Helper.SetAuditFields(applicant.ApplicantId, applicant, _userContext.CurrentUser.UserId);

                if (applicant.ApplicantId == 0)
                    await _dbContext.Applicants.AddAsync(applicant);
                else
                    _dbContext.Applicants.Update(applicant);

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

            }
            else
                return false;

            #endregion Applicant

            return true;
        }
    }
}
