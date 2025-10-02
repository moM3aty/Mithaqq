using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mithaqq.Data;
using Mithaqq.Models;
using Mithaqq.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Mithaqq.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                LatestPosts = await _context.BlogPosts.OrderByDescending(p => p.PublishDate).Take(3).ToListAsync()
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Global()
        {
            var viewModel = await GetCompanyStoreViewModel("Mithaqq Global");
            if (viewModel == null) return NotFound();
            return View("Global", viewModel);
        }

        public async Task<IActionResult> Nile()
        {
            var viewModel = await GetCompanyStoreViewModel("Mithaqq Nile");
            if (viewModel == null) return NotFound();
            return View("Nile", viewModel);
        }

        public async Task<IActionResult> Innovation()
        {
            var viewModel = await GetCompanyStoreViewModel("Mithaqq Innovation");
            if (viewModel == null) return NotFound();
            return View("Innovation", viewModel);
        }

        public async Task<IActionResult> EasyWay()
        {
            var viewModel = await GetCompanyStoreViewModel("Mithaqq Easy Way");
            if (viewModel == null) return NotFound();
            return View("EasyWay", viewModel);
        }

        private async Task<CompanyStoreViewModel> GetCompanyStoreViewModel(string companyName)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Name == companyName);
            if (company == null) return null;

            var trainingFields = await _context.Courses
                .Where(c => c.CompanyId == company.Id)
                .GroupBy(c => c.Category.Name)
                .Select(g => new TrainingFieldViewModel
                {
                    CategoryName = g.Key,
                    CourseCount = g.Count()
                }).ToListAsync();

            foreach (var field in trainingFields)
            {
                field.IconClass = GetIconForCategory(field.CategoryName);
            }

            var viewModel = new CompanyStoreViewModel
            {
                Company = company,
                Products = await _context.Products
                    .Where(p => p.CompanyId == company.Id)
                    .Include(p => p.Category)
                    .ToListAsync(),
                Courses = await _context.Courses
                    .Where(c => c.CompanyId == company.Id)
                    .Include(c => c.Category)
                    .ToListAsync(),
                TravelPackages = companyName == "Mithaqq Easy Way"
                    ? await _context.TravelPackages.ToListAsync()
                    : new List<TravelPackage>(),
                TrainingFields = trainingFields
            };

            return viewModel;
        }

        private string GetIconForCategory(string categoryName)
        {
            return categoryName.ToLower() switch
            {
                "business & management" => "fas fa-briefcase",
                "technology & it" => "fas fa-laptop-code",
                "marketing & sales" => "fas fa-bullhorn",
                "finance & accounting" => "fas fa-chart-pie",
                "human resources" => "fas fa-users",
                "soft skills" => "fas fa-handshake",
                _ => "fas fa-chalkboard-teacher",
            };
        }

        public async Task<IActionResult> BlogDetail(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }
            return View(blogPost);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

