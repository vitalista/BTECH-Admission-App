using BTECH_APP.Models.Admin.Dashboard;

namespace BTECH_APP.Services.Admin.Interfaces
{
    public interface IDashboardService
    {
        Task<StatisticsModel> Statistic();
    }
}
