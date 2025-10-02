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
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> PostReview([FromBody] ReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Check if user has purchased the item, unless they are an Admin
            bool hasPurchased = isAdmin; // Admin can review anything
            if (!isAdmin)
            {
                if (model.ProductId.HasValue)
                {
                    hasPurchased = await _context.OrderDetails
                        .AnyAsync(od => od.ProductId == model.ProductId.Value && od.Order.UserId == userId && od.Order.Status == "Completed");
                }
                else if (model.CourseId.HasValue)
                {
                    hasPurchased = await _context.OrderDetails
                       .AnyAsync(od => od.CourseId == model.CourseId.Value && od.Order.UserId == userId && (od.Order.Status == "Completed" || od.Order.Status == "Pending Approval"));
                }
            }


            if (!hasPurchased)
            {
                return BadRequest("You can only review items you have purchased.");
            }

            // Check if user already reviewed this specific item
            var existingReviewQuery = _context.Reviews.Where(r => r.UserId == userId);
            if (model.ProductId.HasValue)
            {
                existingReviewQuery = existingReviewQuery.Where(r => r.ProductId == model.ProductId.Value);
            }
            else if (model.CourseId.HasValue)
            {
                existingReviewQuery = existingReviewQuery.Where(r => r.CourseId == model.CourseId.Value);
            }

            var existingReview = await existingReviewQuery.FirstOrDefaultAsync();


            if (existingReview != null)
            {
                return BadRequest("You have already submitted a review for this item.");
            }

            var review = new Review
            {
                ProductId = model.ProductId,
                CourseId = model.CourseId,
                UserId = userId,
                Stars = model.Stars,
                Comment = model.Comment,
                DatePosted = System.DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Recalculate average rating
            await UpdateAverageRating(model.ProductId, model.CourseId);

            // Return a simple, flat object to avoid circular references
            var user = await _userManager.FindByIdAsync(userId);
            var result = new
            {
                id = review.Id,
                stars = review.Stars,
                comment = review.Comment,
                datePosted = review.DatePosted,
                userName = user.FirstName + " " + user.LastName
            };

            return Ok(result);
        }

        private async Task UpdateAverageRating(int? productId, int? courseId)
        {
            if (productId.HasValue)
            {
                var product = await _context.Products.Include(p => p.Reviews).FirstOrDefaultAsync(p => p.Id == productId.Value);
                if (product != null)
                {
                    product.AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Stars) : 0;
                    product.RatingCount = product.Reviews.Count;
                    await _context.SaveChangesAsync();
                }
            }
            else if (courseId.HasValue)
            {
                var course = await _context.Courses.Include(c => c.Reviews).FirstOrDefaultAsync(c => c.Id == courseId.Value);
                if (course != null)
                {
                    course.AverageRating = course.Reviews.Any() ? course.Reviews.Average(r => r.Stars) : 0;
                    course.RatingCount = course.Reviews.Count;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}

