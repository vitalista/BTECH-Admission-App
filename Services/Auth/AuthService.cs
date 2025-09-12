using BTECH_APP.Entities.Auth;
using BTECH_APP.Models;
using BTECH_APP.Models.Auth;
using BTECH_APP.Services.Auth.Interfaces;
using BTECH_APP.Services.Email;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using static BTECH_APP.Enums;

namespace BTECH_APP.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly BTECHDbContext _dbContext;
        private readonly UserContext _userContext;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthService(BTECHDbContext dbContext, UserContext userContext, IEmailService emailService, IHttpContextAccessor context)
        {
            _dbContext = dbContext;
            _userContext = userContext;
            _emailService = emailService;
            _httpContextAccessor = context;
        }
        public async Task GetCurrentUser()
        {
            CurrentUserModel currentUser = new();

            var user = _httpContextAccessor.HttpContext?.User;

            if (user != null || user?.Identity != null || user.Identity.IsAuthenticated)
            {
                currentUser.Name = user.FindFirstValue(ClaimTypes.Name);

                var roleString = user.FindFirstValue(ClaimTypes.Role);

                if (Enum.TryParse<RoleTypes>(roleString, out var role))
                    currentUser.Role = role;
                else
                    currentUser.Role = RoleTypes.NotUser;

                currentUser.UserId = int.TryParse(user.FindFirstValue("UserId"), out var userId) ? userId : 0;
                currentUser.PersonId = int.TryParse(user.FindFirstValue("PersonId"), out var personId) ? personId : 0;
                currentUser.ApplicantId = int.TryParse(user.FindFirstValue("ApplicantId"), out var applicantId) ? applicantId : 0;
                currentUser.ExpiresIn = DateTime.TryParse(user.FindFirstValue("ExpiresIn"), out var exp) ? exp : DateTime.MinValue;

                var applicant = await _dbContext.Applicants.AsNoTracking()
                               .Where(x => x.UserId == x.UserId)
                               .FirstOrDefaultAsync();

                if (applicant != null)
                {
                    currentUser.ApplicantId = applicant.ApplicantId;
                    currentUser.ApplicantStatus = applicant.Status;
                }
                else
                {
                    currentUser.ApplicantId = 0;
                    currentUser.ApplicantStatus = ApplicantStatus.Draft;
                }
            }

            _userContext.CurrentUser = currentUser;
        }
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        public async Task<bool> ValidateAccount(LoginModel model)
        {
            var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Email.Trim().ToLower().Equals(model.Email.Trim().ToLower()));

            if (user == null) return false;

            var applicant = await _dbContext.Applicants.AsNoTracking().FirstOrDefaultAsync(applicant => applicant.UserId == user.UserId);

            if (!VerifyPassword(model.Password, user.Password)) return false;

            var information = await _dbContext.FindAsync<UserInformationEntity>(user?.PersonId);
            if (information == null) return false;

            return true;
        }
        public async Task<bool> LoginAsync(LoginModel model)
        {
            var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Email.Trim().ToLower().Equals(model.Email.Trim().ToLower()));

            if (user == null) return false;

            var applicant = await _dbContext.Applicants.AsNoTracking().FirstOrDefaultAsync(applicant => applicant.UserId == user.UserId);

            if (!VerifyPassword(model.Password, user.Password)) return false;

            var information = await _dbContext.FindAsync<UserInformationEntity>(user?.PersonId);
            if (information == null) return false;

            var name = information == null ? "Unknown User" : information.FullName;
            var userId = user == null ? 0 : user.UserId;
            var personId = information == null ? 0 : information.PersonId;
            var applicantId = applicant == null ? 0 : applicant.ApplicantId;
            var applicantStatus = applicant == null ? ApplicantStatus.Draft : applicant.Status;
            var userRole = user == null ? RoleTypes.NotUser : user.Role;

            var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, name),
                            new Claim("UserId",userId.ToString()),
                            new Claim("PersonId",personId.ToString()),
                            new Claim("ApplicantId",applicantId.ToString()),
                            new Claim("UserId",userId.ToString()),
                            new Claim("UserId",userId.ToString()),
                            new Claim("ExpiresIn",DateTime.UtcNow.AddHours(24).ToString()),
                            new Claim("ApplicantStatus",applicantStatus.GetDisplayName()),
                            new Claim(ClaimTypes.Role,userRole.GetDisplayName()),
            };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await _httpContextAccessor.HttpContext.SignInAsync("MyCookieAuth", principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            });

            return true;
        }
        public async Task LogoutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync("MyCookieAuth");
        }
        public async Task<(bool, string, RegisterModel)> RegisterAsync(RegisterModel model)
        {
            bool isEmailUnique = !await _dbContext.Users.AnyAsync(user => (user.Email.Trim().ToLower()).Equals(model.Email.Trim().ToLower()));

            if (!isEmailUnique)
            {
                model.Email = string.Empty;
                return (false, "Email is already registered!", model);
            }

            var infoUnique = await (from info in _dbContext.UserInformations.AsNoTracking()
                                    where info.FirstName.Contains(model.FirstName)
                                    && info.LastName.Contains(model.LastName)
                                    && info.BirthDate.Date == (model.BirthDate ?? DateTime.Now).Date
                                    && (string.IsNullOrEmpty(model.MiddleName) || info.MiddleName.Contains(model.MiddleName))
                                    && (string.IsNullOrEmpty(model.LastName) || info.LastName.Contains(model.LastName))

                                    select info).FirstOrDefaultAsync();

            if (infoUnique != null)
            {
                model.IsAgree = false;
                return (false, "Your Details is already registered!", model);
            }

            UserInformationEntity information = new()
            {
                FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.FirstName.Trim().ToLower()),
                MiddleName = string.IsNullOrWhiteSpace(model.MiddleName) ? null : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.MiddleName.Trim().ToLower()),
                LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.LastName.Trim().ToLower()),
                Suffix = string.IsNullOrWhiteSpace(model.Suffix) ? null : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.Suffix.Trim().ToLower()),
                BirthDate = model.BirthDate ?? DateTime.Now,
            };

            await _dbContext.UserInformations.AddAsync(information);
            await _dbContext.SaveChangesAsync();

            Random random = new Random();
            int sixDigitNumber = random.Next(100000, 1000000);

            UserEntity user = new()
            {
                PersonId = information.PersonId,
                Email = model.Email.Trim(),
                Password = HashPassword(sixDigitNumber.ToString()),
                IsDefaultPassword = true,
                Role = RoleTypes.Applicant,
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            _ = Task.Run(() => _emailService.SendEmail(
                user.Email,
                information.FullName,
                "Account Details",
                $"We are pleased to inform you that your account has been successfully created.<br/>" +
                $"Your account password is {sixDigitNumber}, you can now login."
            ));


            return (true, "Account created successfully, Please check your email!", model);
        }

        public async Task<bool> Relogin(int userId)
        {
            var user = await _dbContext.Users.AsNoTracking().FirstAsync(x => x.UserId == userId);

            if (user == null) return false;

            var information = await _dbContext.FindAsync<UserInformationEntity>(user.PersonId);
            if (information == null) return false;

            var name = information == null ? "Unknown User" : information.FullName;
            var personId = information == null ? 0 : information.PersonId;
            var applicantId = 0;
            var applicantStatus = ApplicantTypes.NotSet;
            var userRole = user == null ? RoleTypes.NotUser : user.Role;

            await _httpContextAccessor.HttpContext.SignOutAsync("MyCookieAuth");

            var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, name),
                            new Claim("UserId",userId.ToString()),
                            new Claim("PersonId",personId.ToString()),
                            new Claim("ApplicantId",applicantId.ToString()),
                            new Claim("UserId",userId.ToString()),
                            new Claim("UserId",userId.ToString()),
                            new Claim("ExpiresIn",DateTime.UtcNow.AddHours(24).ToString()),
                            new Claim("ApplicantStatus",applicantStatus.GetDisplayName()),
                            new Claim(ClaimTypes.Role,userRole.GetDisplayName()),
            };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await _httpContextAccessor.HttpContext.SignInAsync("MyCookieAuth", principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            });

            return true;
        }
    }
}
