using Microsoft.AspNetCore.Components.Forms;
using static BTECH_APP.Enums;

namespace BTECH_APP.Helpers
{
    public class Helper
    {

        public static string GetCurrentSchoolYear()
        {
            return $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.AddYears(1).Year}";
        }

        public static string GenerateApplicantNo(SemesterTypes semester, int nextSeqNo)
        {
            int year = DateTime.Now.Year;

            string paddedNumber = nextSeqNo.ToString("D9");
            string semesterFormatted = ((int)semester).ToString("D2");

            return $"App-{year}{semesterFormatted}-{paddedNumber}";
        }

        public static string? FormatDate(DateTime? date) =>
            date?.ToString("MMM dd, yyyy");

        public static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
                age--;

            return age;
        }

        public static async Task<(string? FileName, FileTypes FileType, string? FilePath)> UploadFile(IBrowserFile file, string folderName)
        {
            if (file != null)
            {
                using var stream = file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 1024);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                byte[] imageData = memoryStream.ToArray();

                string fileName = file.Name;
                string extension = Path.GetExtension(fileName)?.ToLowerInvariant();
                string webPath = string.Empty;

                FileTypes fileType = extension switch
                {
                    ".jpg" or ".jpeg" or ".png" => FileTypes.Image,
                    ".pdf" => FileTypes.PDF,
                    ".doc" or ".docx" => FileTypes.Docs,
                    ".xls" or ".xlsx" => FileTypes.Excel,
                    _ => FileTypes.Invalid
                };

                if (fileType != FileTypes.Invalid)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    string folderPath = Path.Combine("wwwroot", $"files/{folderName}");
                    string physicalPath = Path.Combine(folderPath, uniqueFileName);

                    Directory.CreateDirectory(folderPath);

                    await File.WriteAllBytesAsync(physicalPath, imageData);

                    webPath = $"files/{folderName}/{uniqueFileName}";

                    return (fileName, fileType, webPath);
                }

                return (fileName, FileTypes.Invalid, webPath);
            }

            return (null, FileTypes.Invalid, null);
        }

        public static dynamic SetAuditFields(int primaryKey, dynamic entity, int currentUserId)
        {
            if (primaryKey == 0)
            {
                entity.CreatedByUserId = currentUserId;
                entity.CreatedDate = DateTime.UtcNow;
            }

            entity.ModifiedByUserId = currentUserId;
            entity.ModifiedDate = DateTime.UtcNow;

            return entity;
        }

        public static bool ObjectIsDifferent<Model>(Model originalModel, Model newModel)
        {
            if (originalModel == null || newModel == null) return true;

            foreach (var prop in typeof(Model).GetProperties())
            {
                var originalValue = prop.GetValue(originalModel);
                var newValue = prop.GetValue(newModel);

                if (!Equals(originalValue, newValue))
                    return true;
            }

            return false;
        }

        public static string StatusEmailMessage(ApplicantStatus status, string currentUserName, string? remarks, DateTime? scheduleDate)
        {
            string message = string.Empty;

            switch (status)
            {
                case ApplicantStatus.Returned:
                    message = $"<b>{currentUserName}</b> returned your requirements{(string.IsNullOrEmpty(remarks) ? "." : $", remarks: {remarks}")}.";
                    break;
                case ApplicantStatus.Cancelled:
                    message = $"<b>{currentUserName}</b> cancelled your application{(string.IsNullOrEmpty(remarks) ? "." : $", remarks: {remarks}")}.";
                    break;
                case ApplicantStatus.Endorsed:
                    message = $"<b>{currentUserName}</b> endorse your application to admin{(string.IsNullOrEmpty(remarks) ? "." : $", remarks: {remarks}")}.";
                    break;
                case ApplicantStatus.ReturnedToVerifier:
                    message = $"<b>{currentUserName}</b> returned your application to the verifier{(string.IsNullOrEmpty(remarks) ? "." : $", remarks: {remarks}")}.";
                    break;
                case ApplicantStatus.ForSchedule:
                    message = $"<b>{currentUserName}</b> marked your application as ready for scheduling{(string.IsNullOrEmpty(remarks) ? "." : $", remarks: {remarks}")}.";
                    break;
                case ApplicantStatus.Scheduled:
                    message = $"<b>{currentUserName}</b> scheduled your exam on {Helper.FormatDate(scheduleDate)}. Please arrive at least 30 minutes early{(string.IsNullOrEmpty(remarks) ? "." : $", remarks: {remarks}")}.";
                    break;
                case ApplicantStatus.Recommending:
                    message = $"<b>{currentUserName}</b> has set your application for recommending. Please log in and select your desired program for admission{(string.IsNullOrEmpty(remarks) ? "." : $", remarks: {remarks}")}";
                    break;
                case ApplicantStatus.Rejected:
                    message = $"<b>{currentUserName}</b> has reviewed your application and, unfortunately, it has been rejected{(string.IsNullOrEmpty(remarks) ? "." : $".<br/>remarks: {remarks}")}";
                    break;
            }

            return message;
        }

        public static string StatusConfimationMessage(ApplicantStatus status) =>
        status switch
        {
            ApplicantStatus.Returned => "Are you sure you want to return this application to the applicant?",
            ApplicantStatus.Cancelled => "Are you sure you want to cancel this application?",
            ApplicantStatus.Endorsed => "Are you sure you want to endorse this application for admin approval?",
            ApplicantStatus.ReturnedToVerifier => "Are you sure you want to return this application to the verifier?",
            ApplicantStatus.ForSchedule => "Are you sure you want to mark this application as ready for scheduling?",
            ApplicantStatus.Scheduled => "Are you sure you want to mark this application as scheduled?",
            ApplicantStatus.Recommending => "Are you sure you want to mark this application for recommending?",
            ApplicantStatus.Rejected => "Are you sure you want to reject this application?",
            ApplicantStatus.Admitted => "Evaluating the applicant",
            _ => "Are you sure you want to update this application?"
        };

        public static string StatusSuccessMessage(ApplicantStatus status) =>
        status switch
        {
            ApplicantStatus.Returned => "Successfully returned the application to the applicant.",
            ApplicantStatus.Cancelled => "Application successfully cancelled.",
            ApplicantStatus.Endorsed => "Application successfully endorsed for admin approval.",
            ApplicantStatus.ReturnedToVerifier => "Application successfully returned to the verifier.",
            ApplicantStatus.ForSchedule => "Application marked as ready for scheduling.",
            ApplicantStatus.Scheduled => "Application successfully scheduled.",
            ApplicantStatus.Recommending => "Application marked for recommending.",
            ApplicantStatus.Rejected => "Application successfully rejected.",
            ApplicantStatus.Admitted => "Applicant successfully marked as admitted.",
            _ => "Application updated successfully."
        };

    }
}
