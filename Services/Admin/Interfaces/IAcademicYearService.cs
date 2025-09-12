using BTECH_APP.Models.Admin.Dashboard;

namespace BTECH_APP.Services.Admin.Interfaces
{
    public interface IAcademicYearService
    {
        Task<SaveAcademicYearModel?> GetCurrentAcademic();
        Task<bool> Toggle(SaveAcademicYearModel model);
        Task<(string schoolYear, string semester, bool IsActive)> IsAcademicYearOpen();
        Task<IEnumerable<string>> Lookup();
    }
}
