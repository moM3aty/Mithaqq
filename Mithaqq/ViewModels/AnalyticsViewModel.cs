using System.Collections.Generic;

namespace Mithaqq.ViewModels
{


    public class AnalyticsViewModel
    {
        public List<ChartData> MonthlySales { get; set; }
        public List<ChartData> SalesByCategory { get; set; }
        public List<TopItemData> TopProducts { get; set; }
        public List<TopItemData> TopCourses { get; set; }
        public List<ChartData> MonthlyEnrollments { get; set; }
        public List<ChartData> CompletionRates { get; set; }
    }

    public class ChartData
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }

    public class TopItemData
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }

}
