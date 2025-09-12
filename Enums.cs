using System.ComponentModel.DataAnnotations;

namespace BTECH_APP
{
    public static class Enums
    {

        public enum ApplicantTypes
        {
            [Display(Name = "Not Set")] NotSet = 0,
            [Display(Name = "Freshmen")] Freshmen = 1,
            [Display(Name = "Transferee")] Transferee = 2,
            [Display(Name = "ALS Graduate")] AlsGraduate = 3
        }

        public enum ApplicantStatus
        {
            [Display(Name = "Draft")] Draft = 1,
            [Display(Name = "For Requirements")] ForRequirements = 2,
            [Display(Name = "Submitted")] Submitted = 3,
            [Display(Name = "Re-Submitted")] Resubmitted = 4,
            [Display(Name = "Returned")] Returned = 5,
            [Display(Name = "Cancelled")] Cancelled = 6,
            [Display(Name = "Endorsed")] Endorsed = 7,
            [Display(Name = "Returned to Verifier")] ReturnedToVerifier = 8,
            [Display(Name = "For Schedule")] ForSchedule = 9,
            [Display(Name = "Scheduled")] Scheduled = 10,
            [Display(Name = "Recommending")] Recommending = 11,
            [Display(Name = "Rejected")] Rejected = 12,
            [Display(Name = "Admitted")] Admitted = 13,
        }

        public enum AddressTypes
        {
            [Display(Name = "Permanent Address")] Permanent = 1,
            [Display(Name = "Present Address")] Present = 2
        }

        public enum EducationBackgroundTypes
        {
            [Display(Name = "Elementary")] Elem = 1,
            [Display(Name = "Junior High School")] Junior = 2,
            [Display(Name = "Senior High School")] Senior = 3,
            [Display(Name = "Tertiary")] Tertiary = 4
        }

        public enum FileTypes
        {
            [Display(Name = "Invalid File")] Invalid = 0,
            [Display(Name = "Image")] Image = 1,
            [Display(Name = "PDF")] PDF = 2,
            [Display(Name = "Docs")] Docs = 3,
            [Display(Name = "Excel")] Excel = 4,
        }

        public enum GenderTypes
        {
            [Display(Name = "Not Set")] NotSet = 0,
            [Display(Name = "Male")] Male = 1,
            [Display(Name = "Female")] Female = 2,
        }

        public enum RoleTypes
        {
            [Display(Name = "Not User")] NotUser = 0,
            [Display(Name = "Admin")] Admin = 1,
            [Display(Name = "Verifier")] Verifier = 2,
            [Display(Name = "Scheduler")] Scheduler = 3,
            [Display(Name = "Applicant")] Applicant = 4
        }

        public enum CivilStatus
        {
            [Display(Name = "Not Set")] NotSet = 0,
            [Display(Name = "Single")] Single = 1,
            [Display(Name = "Married")] Married = 2
        }

        public enum SelectedProgramTypes
        {
            [Display(Name = "First Choice")] First = 1,
            [Display(Name = "Second Choice")] Second = 2,
            [Display(Name = "Recommended")] Recommended = 3
        }

        public enum SemesterTypes
        {
            [Display(Name = "First Semester")] FirstSemester = 1,
            [Display(Name = "Second Semester")] SecondSemester = 2,
        }

        public static string GetDisplayName(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));
            return attribute?.Name ?? value.ToString();
        }

    }
}
