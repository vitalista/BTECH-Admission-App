using AutoMapper;
using BTECH_APP.Entities.Auth;
using BTECH_APP.Helpers;
using BTECH_APP.Models;
using BTECH_APP.Models.Admin.UserManagement;
using BTECH_APP.Services.Admin.Interfaces;
using BTECH_APP.Services.Email;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.Globalization;

namespace BTECH_APP.Services.Admin
{
    public class UserManagementService : IUserManagementService
    {
        private readonly BTECHDbContext _dbContext;
        private readonly UserContext _userContext;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public UserManagementService(BTECHDbContext dbContext, UserContext userContext, IMapper mapper, IEmailService emailService)
        {
            _dbContext = dbContext;
            _userContext = userContext;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<List<ListUserManagementModel>> List()
        {
            var query = from user in _dbContext.Users.AsNoTracking()

                        join userInformation in _dbContext.UserInformations.AsNoTracking()
                            on user.PersonId equals userInformation.PersonId into informationGroup
                        from userInformation in informationGroup.DefaultIfEmpty()

                        join modifiedBy in _dbContext.Users.AsNoTracking()
                            on user.ModifiedByUserId equals modifiedBy.UserId into modifiedByGroup
                        from modifiedBy in modifiedByGroup.DefaultIfEmpty()

                        join modifiedByInformation in _dbContext.UserInformations.AsNoTracking()
                            on modifiedBy.PersonId equals modifiedByInformation.PersonId into modifiedByInformationGroup
                        from modifiedByInformation in modifiedByInformationGroup.DefaultIfEmpty()

                        where !user.Deleted && user.UserId != 1
                        && user.UserId != _userContext.CurrentUser.UserId
                        && user.Role != Enums.RoleTypes.Applicant

                        select new ListUserManagementModel
                        {
                            PersonId = userInformation.PersonId,
                            UserId = user.UserId,
                            Name = userInformation.FullName,
                            BirthDate = userInformation.BirthDate,
                            Email = user.Email,
                            Role = user.Role,
                            IsActive = user.IsActive,
                            Status = user.IsActive ? "Active" : "Inactive",
                            ModifiedByUserId = user.ModifiedByUserId,
                            ModifiedByPersonId = modifiedByInformation.PersonId,
                            ModifiedByName = modifiedByInformation.FullName,
                            ModifiedDate = user.ModifiedDate,
                        };

            return await query.ToListAsync();
        }
        public async Task<SaveUserManagementModel?> Find(int userId)
        {
            var query = from user in _dbContext.Users.AsNoTracking()

                        join userInformation in _dbContext.UserInformations.AsNoTracking()
                            on user.PersonId equals userInformation.PersonId into informationGroup
                        from userInformation in informationGroup.DefaultIfEmpty()

                        where !user.Deleted
                        && user.UserId == userId
                        select new SaveUserManagementModel
                        {
                            PersonId = userInformation.PersonId,
                            UserId = user.UserId,
                            FirstName = userInformation.FirstName,
                            MiddleName = userInformation.MiddleName,
                            LastName = userInformation.LastName,
                            Suffix = userInformation.Suffix,
                            Email = user.Email,
                            BirthDate = userInformation.BirthDate,
                            Role = user.Role,
                            IsActive = user.IsActive,

                        };

            return await query.FirstOrDefaultAsync();
        }
        public async Task<(bool success, string errorMessage)> Save(SaveUserManagementModel model)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                #region Information

                UserInformationEntity userInformationEntity = _mapper.Map<UserInformationEntity>(model);

                userInformationEntity.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.FirstName.Trim().ToLower());
                userInformationEntity.MiddleName = string.IsNullOrWhiteSpace(model.MiddleName) ? null : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.MiddleName.Trim().ToLower());
                userInformationEntity.LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.LastName.Trim().ToLower());
                userInformationEntity.Suffix = string.IsNullOrWhiteSpace(model.Suffix) ? null : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.Suffix.Trim().ToLower());

                if (userInformationEntity.PersonId != 0)
                {
                    var originalUserInfomation = await _dbContext.UserInformations.FirstOrDefaultAsync(x => x.PersonId == userInformationEntity.PersonId);

                    if (originalUserInfomation == null)
                        return (false, "Invalid user information not found!.");

                    userInformationEntity = _mapper.Map(model, originalUserInfomation);
                }

                var infoUnique = await (from info in _dbContext.UserInformations.AsNoTracking()
                                        where info.FirstName.Contains(userInformationEntity.FirstName)
                                        && info.LastName.Contains(userInformationEntity.LastName)
                                        && info.BirthDate.Date == (model.BirthDate ?? DateTime.Now).Date
                                        && (string.IsNullOrEmpty(model.MiddleName) || info.MiddleName.Contains(model.MiddleName))
                                        && (string.IsNullOrEmpty(model.LastName) || info.LastName.Contains(model.LastName))
                                        && info.PersonId != userInformationEntity.PersonId
                                        select info).FirstOrDefaultAsync();

                if (infoUnique != null)
                    return (false, "Your Details is already registered!");

                Helper.SetAuditFields(userInformationEntity.PersonId, userInformationEntity, _userContext.CurrentUser.UserId);

                if (userInformationEntity.PersonId == 0)
                    await _dbContext.UserInformations.AddAsync(userInformationEntity);
                else
                    _dbContext.UserInformations.Update(userInformationEntity);

                await _dbContext.SaveChangesAsync();
                #endregion Information

                #region User
                UserEntity userEntity = _mapper.Map<UserEntity>(model);

                if (userEntity.UserId != 0)
                {
                    var originalUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userEntity.UserId);

                    if (originalUser == null)
                        return (false, "Invalid user not found!.");

                    userEntity = _mapper.Map(model, originalUser);
                }

                userEntity.PersonId = userInformationEntity.PersonId;

                bool isEmailUnique = !await _dbContext.Users.AnyAsync(user => (user.Email.Trim().ToLower()).Equals(model.Email.Trim().ToLower()) && user.UserId != userEntity.UserId);

                if (!isEmailUnique)
                    return (false, "Email is already registered!");

                Helper.SetAuditFields(userEntity.UserId, userEntity, _userContext.CurrentUser.UserId);

                var sixDigitNumber = 0;
                if (userEntity.UserId == 0)
                {
                    Random random = new Random();
                    sixDigitNumber = random.Next(100000, 1000000);
                    userEntity.Password = BCrypt.Net.BCrypt.HashPassword(sixDigitNumber.ToString());
                    userEntity.IsDefaultPassword = true;

                    await _dbContext.Users.AddAsync(userEntity);
                }
                else
                    _dbContext.Users.Update(userEntity);

                await _dbContext.SaveChangesAsync();
                #endregion User

                await transaction.CommitAsync();

                if (model.UserId == 0)
                {
                    var user = await _emailService.GetUserEmail(userEntity.UserId);

                    string message = $"<b>{_userContext.CurrentUser.Name}</b> created your account for BTECH Admission System as " +
                        $"{userEntity.Role.GetDisplayName()}.<br/> Your account password is {sixDigitNumber}, you can now login.";

                    if (!string.IsNullOrEmpty(user.email) && !string.IsNullOrEmpty(user.fullname))
                    {
                        _ = Task.Run(() => _emailService.SendEmail(
                              user.email,
                              user.fullname,
                              "Account Details",
                              message
                          ));
                    }
                }

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "An error occurred while saving: " + ex.Message);
            }
            return (true, string.Empty);
        }

        public async Task Delete(int userId)
        {
            var entity = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);

            if (entity != null)
            {
                entity.Deleted = true;
                entity.DeletedDate = DateTime.UtcNow;
                entity.DeletedByUserId = _userContext.CurrentUser.UserId;

                _dbContext.SaveChanges();
            }
        }

        public async Task<bool> ResetPassword(int userId)
        {
            var entity = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);

            if (entity != null)
            {
                Random random = new Random();
                int sixDigitNumber = random.Next(100000, 1000000);

                entity.IsDefaultPassword = true;
                entity.Password = BCrypt.Net.BCrypt.HashPassword(sixDigitNumber.ToString());

                _dbContext.Users.Update(entity);
                await _dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> ChangePassword(SaveChangePasswordUserManagementModel model)
        {
            var entity = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == model.UserId);

            if (entity != null)
            {
                if (BCrypt.Net.BCrypt.Verify(model.OldPassword, entity.Password))
                {
                    entity.Password = BCrypt.Net.BCrypt.HashPassword(model.ConfirmPassword);

                    _dbContext.Update(entity);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                else
                    return false;
            }
            return false;
        }
    }
}
