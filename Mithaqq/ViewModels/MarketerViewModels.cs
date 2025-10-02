using Mithaqq.Models;
using System;
using System.Collections.Generic;

namespace Mithaqq.ViewModels
{
    public class MarketerDashboardViewModel
    {
        public string MarketerName { get; set; }
        public string ReferralCode { get; set; }
        public int TotalReferredUsers { get; set; }
        public decimal TotalSalesFromReferrals { get; set; }
        public decimal TotalCommissionEarned { get; set; }
        public List<ApplicationUser> RecentReferredUsers { get; set; }
        public List<CommissionViewModel> RecentCommissions { get; set; }
    }

    public class CommissionViewModel
    {
        public string CustomerName { get; set; }
        public decimal SaleAmount { get; set; }
        public decimal CommissionEarned { get; set; }
        public DateTime OrderDate { get; set; }
    }
}

