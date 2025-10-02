using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mithaqq.Data;
using Mithaqq.Models;
using Mithaqq.Services;
using Mithaqq.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Mithaqq.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BunnyService _bunnyService;

        public StoreController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, BunnyService bunnyService)
        {
            _context = context;
            _userManager = userManager;
            _bunnyService = bunnyService;
        }

        public async Task<IActionResult> Index()
        {
            var allProducts = await _context.Products.Include(p => p.Company).Include(p => p.Category).ToListAsync();
            var allCourses = await _context.Courses.Include(c => c.Company).Include(c => c.Category).ToListAsync();
            var allTravelPackages = await _context.TravelPackages.Include(tp => tp.Company).ToListAsync();


            var categories = await _context.Categories
                                        .Where(c => c.Products.Any() || c.Courses.Any())
                                        .OrderBy(c => c.Name)
                                        .ToListAsync();

            var viewModel = new StoreViewModel
            {
                AllProducts = allProducts,
                AllCourses = allCourses,
                AllTravelPackages = allTravelPackages,
                Categories = categories
            };
            return View(viewModel);
        }

        public async Task<IActionResult> ProductDetails(int? id)
        {
            ViewData["NavbarSolid"] = true;
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Company)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var reviews = await _context.Reviews
                .Where(r => r.ProductId == id)
                .Include(r => r.User)
                .OrderByDescending(r => r.DatePosted)
                .ToListAsync();

            var userId = _userManager.GetUserId(User);
            var userHasPurchased = false;
            var userHasReviewed = false;
            if (User.Identity.IsAuthenticated)
            {
                userHasPurchased = await _context.OrderDetails
                    .AnyAsync(od => od.ProductId == id && od.Order.UserId == userId && od.Order.Status == "Completed");
                userHasReviewed = await _context.Reviews.AnyAsync(r => r.ProductId == id && r.UserId == userId);
            }

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Reviews = reviews,
                UserHasPurchased = userHasPurchased,
                UserHasReviewed = userHasReviewed
            };

            return View(viewModel);
        }

        public async Task<IActionResult> CourseDetails(int? id)
        {
            ViewData["NavbarSolid"] = true;
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Company)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            var reviews = await _context.Reviews
                .Where(r => r.CourseId == id)
                .Include(r => r.User)
                .OrderByDescending(r => r.DatePosted)
                .ToListAsync();

            var userId = _userManager.GetUserId(User);
            var userHasPurchased = false;
            var userHasReviewed = false;
            if (User.Identity.IsAuthenticated)
            {
                userHasPurchased = await _context.OrderDetails
                    .AnyAsync(od => od.CourseId == id && od.Order.UserId == userId && (od.Order.Status == "Completed" || od.Order.Status == "Pending Approval"));
                userHasReviewed = await _context.Reviews.AnyAsync(r => r.CourseId == id && r.UserId == userId);
            }

            var viewModel = new CourseDetailViewModel
            {
                Course = course,
                Reviews = reviews,
                UserHasPurchased = userHasPurchased,
                UserHasReviewed = userHasReviewed
            };

            return View(viewModel);
        }

        public async Task<IActionResult> TravelPackageDetails(int? id)
        {
            ViewData["NavbarSolid"] = true;
            if (id == null) return NotFound();
            var package = await _context.TravelPackages.Include(p => p.Company).FirstOrDefaultAsync(p => p.Id == id);
            if (package == null) return NotFound();
            return View(package);
        }


        [Authorize]
        public async Task<IActionResult> Course(int? id)
        {
            ViewData["NavbarSolid"] = true;
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var isPurchased = await _context.OrderDetails
                .AnyAsync(od => od.CourseId == id && od.Order.UserId == user.Id && (od.Order.Status == "Completed" || od.Order.Status == "Pending Approval"));

            var isAdmin = User.IsInRole("Admin");

            if (!isPurchased && !isAdmin)
            {
                return RedirectToAction("CourseCheckout", "Checkout", new { courseId = id });
            }

            var course = await _context.Courses
                .Include(c => c.Lessons.OrderBy(l => l.Order))
                .FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();

            var viewModel = new CourseContentViewModel
            {
                CourseName = course.Name,
                Lessons = course.Lessons.Select(l => new LessonViewModel
                {
                    Id = l.Id,
                    Title = l.Title,
                    SecureVideoUrl = _bunnyService.GenerateSecureUrl(l.BunnyVideoId, l.BunnyLibraryId)
                }).ToList(),
                UserIdentifier = user.Email // Pass user's email for watermarking
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int? productId, int? courseId, int? travelPackageId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "User not logged in." });
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, DateCreated = DateTime.UtcNow };
                _context.Carts.Add(cart);
            }

            string itemName = "";

            if (productId.HasValue)
            {
                var productToAdd = await _context.Products.FindAsync(productId.Value);
                if (productToAdd != null)
                {
                    var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId.Value);
                    if (cartItem != null) cartItem.Quantity += quantity;
                    else
                    {
                        cart.CartItems.Add(new CartItem { ProductId = productId.Value, Quantity = quantity, Price = productToAdd.SalePrice ?? productToAdd.Price });
                    }
                    itemName = productToAdd.Name;
                }
            }
            else if (courseId.HasValue)
            {
                if (!cart.CartItems.Any(ci => ci.CourseId == courseId.Value))
                {
                    var courseToAdd = await _context.Courses.FindAsync(courseId.Value);
                    if (courseToAdd != null)
                    {
                        cart.CartItems.Add(new CartItem { CourseId = courseId.Value, Quantity = 1, Price = courseToAdd.SalePrice ?? courseToAdd.Price });
                        itemName = courseToAdd.Name;
                    }
                }
            }
            else if (travelPackageId.HasValue)
            {
                if (!cart.CartItems.Any(ci => ci.TravelPackageId == travelPackageId.Value))
                {
                    var packageToAdd = await _context.TravelPackages.FindAsync(travelPackageId.Value);
                    if (packageToAdd != null)
                    {
                        cart.CartItems.Add(new CartItem { TravelPackageId = travelPackageId.Value, Quantity = 1, Price = packageToAdd.Price });
                        itemName = packageToAdd.Name;
                    }
                }
            }

            await _context.SaveChangesAsync();

            var newCartCount = cart.CartItems.Sum(ci => ci.Quantity);

            return Ok(new { success = true, message = $"'{itemName}' added to cart!", newCount = newCartCount });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCart(int cartItemId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                if (cartItem != null && quantity > 0)
                {
                    cartItem.Quantity = quantity;
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                if (cartItem != null)
                {
                    _context.CartItems.Remove(cartItem);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Cart));
        }

        [Authorize]
        public async Task<IActionResult> Cart()
        {
            ViewData["NavbarSolid"] = true;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Course)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.TravelPackage)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var viewModel = new CartViewModel();
            if (cart != null)
            {
                viewModel.CartItems = cart.CartItems.ToList();
            }

            return View(viewModel);
        }
    }
}

