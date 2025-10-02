using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mithaqq.Data;
using Mithaqq.Models;
using Mithaqq.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Mithaqq.Controllers
{
    [Authorize(Roles = "Marketer")]
    public class MarketerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MarketerController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var marketer = await _userManager.GetUserAsync(User);
            if (marketer == null)
            {
                return NotFound("Marketer information not found.");
            }

            if (string.IsNullOrEmpty(marketer.ReferralCode))
            {
                marketer.ReferralCode = System.Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                await _userManager.UpdateAsync(marketer);
            }

            var referredUsers = await _context.Users
                .Where(u => u.ReferredBy == marketer.ReferralCode)
                .ToListAsync();

            var referredUserIds = referredUsers.Select(u => u.Id).ToList();

            var ordersFromReferrals = await _context.Orders
                .Where(o => referredUserIds.Contains(o.UserId))
                .Include(o => o.User)
                .ToListAsync();

            var viewModel = new MarketerDashboardViewModel
            {
                MarketerName = $"{marketer.FirstName} {marketer.LastName}",
                ReferralCode = marketer.ReferralCode,
                TotalReferredUsers = referredUsers.Count,
                TotalSalesFromReferrals = ordersFromReferrals.Sum(o => o.OrderTotal),
                TotalCommissionEarned = ordersFromReferrals.Sum(o => o.OrderTotal) * 0.10m, // Assuming a 10% commission
                RecentReferredUsers = referredUsers.OrderByDescending(u => u.Id).Take(5).ToList(),
                RecentCommissions = ordersFromReferrals.OrderByDescending(o => o.OrderDate).Take(5).Select(o => new CommissionViewModel
                {
                    CustomerName = $"{o.User.FirstName} {o.User.LastName}",
                    SaleAmount = o.OrderTotal,
                    CommissionEarned = o.OrderTotal * 0.10m,
                    OrderDate = o.OrderDate
                }).ToList()
            };

            return View(viewModel);
        }
    }
}
