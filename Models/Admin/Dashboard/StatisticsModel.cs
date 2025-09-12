namespace BTECH_APP.Models.Admin.Dashboard
{
    public class StatisticsModel
    {
        public int TotalFreshmen { get; set; }
        public int TotalTransferee { get; set; }
        public int TotalAlsGrad { get; set; }
        public int TotalSubmitted { get; set; }
        public int TotalScheduled { get; set; }
        public int TotalRecommending { get; set; }
        public int TotalAdmitted { get; set; }
        public int TotalRejected { get; set; }

        public double[] Statistic => [TotalSubmitted, TotalScheduled, TotalRecommending, TotalAdmitted, TotalRejected];
    }
}
