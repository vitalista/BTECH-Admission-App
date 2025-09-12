using BTECH_APP.Models.Email;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;


namespace BTECH_APP.Services.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailModel message);
        Task SendEmail(string email, string receiverName, string subject, string messsage);
        Task<(string email, string fullname)> GetApplicantEmail(int applicantId);
        Task<(string email, string fullname)> GetUserEmail(int userId);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettingsModel _settings;
        private readonly BTECHDbContext _dbContext;

        public EmailService(IOptions<EmailSettingsModel> settings, BTECHDbContext dbContext)
        {
            _settings = settings.Value;
            _dbContext = dbContext;
        }

        public async Task<(string email, string fullname)> GetApplicantEmail(int applicantId)
        {
            var query = await (from applicant in _dbContext.Applicants.AsNoTracking()
                               join user in _dbContext.Users.AsNoTracking() on applicant.UserId equals user.UserId
                               join info in _dbContext.UserInformations.AsNoTracking() on user.PersonId equals info.PersonId
                               where applicant.ApplicantId == applicantId
                               select new { user.Email, info.FullName }).FirstAsync();

            return (query.Email, query.FullName);
        }

        public async Task<(string email, string fullname)> GetUserEmail(int userId)
        {
            var query = await (from user in _dbContext.Users.AsNoTracking()
                               join info in _dbContext.UserInformations.AsNoTracking() on user.PersonId equals info.PersonId
                               where user.UserId == userId
                               select new { user.Email, info.FullName }).FirstAsync();

            return (query.Email, query.FullName);
        }

        public async Task SendEmail(string email, string receiverName, string subject, string messsage)
        {
            var body = $@"
                    <div style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
                        <div style='background-color: #f4f4f4; padding: 20px; border-radius: 8px;'>
                            <h2 style='color: #2c3e50;'>Welcome to BTECH Admission!</h2>
                            <p>Dear <strong>{receiverName}</strong>,</p>

                            <p>{messsage}</p>

                            <p>If you have any questions, feel free to reply to this email or contact our support team.</p>

                            <p>Best regards,<br/>
                            <strong>BTECH Admissions Team</strong></p>
                        </div>
                    </div>";

            EmailModel emailModel = new()
            {
                To = { email },
                Subject = subject,
                Body = body,
                IsHtml = true
            };

            await SendEmailAsync(emailModel);
        }

        public async Task SendEmailAsync(EmailModel message)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            foreach (var to in message.To)
                email.To.Add(MailboxAddress.Parse(to));

            email.Subject = message.Subject;

            var builder = new BodyBuilder { HtmlBody = message.Body };
            email.Body = builder.ToMessageBody();

            try
            {
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }


        }
    }
}
