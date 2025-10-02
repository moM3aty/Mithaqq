using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mithaqq.Data;
using Mithaqq.Models;
using Mithaqq.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Mithaqq.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["NavbarSolid"] = true;
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (await _userManager.IsInRoleAsync(user, "Marketer"))
            {
                return RedirectToAction("Index", "Marketer");
            }

            var userOrders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Course)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var purchasedCourseIds = await _context.OrderDetails
                .Where(od => od.Order.UserId == user.Id && od.CourseId.HasValue && (od.Order.Status == "Completed" || od.Order.Status == "Pending Approval"))
                .Select(od => od.CourseId.Value)
                .Distinct()
                .ToListAsync();

            var userCourses = await _context.Courses
                .Where(c => purchasedCourseIds.Contains(c.Id))
                .ToListAsync();

            var viewModel = new ProfileViewModel
            {
                User = user,
                Orders = userOrders,
                Courses = userCourses,
                EditProfileViewModel = new EditProfileViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = user.Address
                },
                ChangePasswordViewModel = new ChangePasswordViewModel()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                user.FirstName = model.EditProfileViewModel.FirstName;
                user.LastName = model.EditProfileViewModel.LastName;
                user.Address = model.EditProfileViewModel.Address;
                await _userManager.UpdateAsync(user);
                TempData["ProfileSuccess"] = "Your profile has been updated.";
            }
            else
            {
                TempData["ProfileError"] = "Failed to update profile.";
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["PasswordError"] = "Model state is invalid.";
                return RedirectToAction(nameof(Index));
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.ChangePasswordViewModel.OldPassword, model.ChangePasswordViewModel.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                TempData["PasswordError"] = "Failed to change password. " + string.Join(" ", changePasswordResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["PasswordSuccess"] = "Your password has been changed.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MyReviews()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reviews = await _context.Reviews
                .Where(r => r.UserId == userId)
                .Include(r => r.Product)
                .Include(r => r.Course)
                .OrderByDescending(r => r.DatePosted)
                .ToListAsync();

            return View(reviews);
        }

        public async Task<IActionResult> Favorites()
        {
            ViewData["NavbarSolid"] = true;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Product).ThenInclude(p => p.Company)
                .Include(f => f.Course).ThenInclude(c => c.Company)
                .ToListAsync();

            return View(favorites);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToFavorites(int? productId, int? courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingFavorite = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && (f.ProductId == productId || f.CourseId == courseId));

            if (existingFavorite == null)
            {
                var favorite = new Favorite
                {
                    UserId = userId,
                    ProductId = productId,
                    CourseId = courseId
                };
                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Favorites");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromFavorites(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Favorites");
        }
    }
}
