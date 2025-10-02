using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Mithaqq.Data;
using Mithaqq.Models;
using Mithaqq.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace Mithaqq.Controllers
{
    [Authorize(Roles = "CompanyAdmin")]
    public class CompanyAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CompanyAdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var userId = _userManager.GetUserId(User);
            // Eagerly load the Company navigation property to ensure it's available
            return await _context.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user?.CompanyId == null)
            {
                return Unauthorized("You are not associated with a company.");
            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            ViewData["IsEasyWayAdmin"] = user.Company?.Name == "Mithaqq Easy Way";
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var user = await GetCurrentUserAsync();
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == user.CompanyId);

            if (course == null) return NotFound();

            var model = new EditCourseViewModel
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Price = course.Price,
                SalePrice = course.SalePrice,
                ImageUrl = course.ImageUrl,
                InstructorName = course.InstructorName,
                IsOnline = course.IsOnline,
                CompanyId = course.CompanyId,
                CategoryId = course.CategoryId,
                Lessons = course.Lessons.OrderBy(l => l.Order).ToList(),
                AllCategories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", course.CategoryId)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(EditCourseViewModel model)
        {
            var user = await GetCurrentUserAsync();
            var course = await _context.Courses.Include(c => c.Lessons).FirstOrDefaultAsync(c => c.Id == model.Id && c.CompanyId == user.CompanyId);
            if (course == null) return NotFound();

            ModelState.Remove("AllCompanies");
            ModelState.Remove("AllCategories");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Reviews");

            for (int i = 0; i < (model.Lessons?.Count ?? 0); i++)
            {
                ModelState.Remove($"Lessons[{i}].Course");
            }

            if (!ModelState.IsValid)
            {
                model.AllCategories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }

            course.Name = model.Name;
            course.Description = model.Description;
            course.Price = model.Price;
            course.SalePrice = model.SalePrice;
            course.InstructorName = model.InstructorName;
            course.CategoryId = model.CategoryId;
            course.IsOnline = model.IsOnline;

            var incomingLessonIds = model.Lessons?.Select(l => l.Id).ToList() ?? new List<int>();
            var lessonsToDelete = course.Lessons.Where(dbLesson => !incomingLessonIds.Contains(dbLesson.Id)).ToList();
            _context.Lessons.RemoveRange(lessonsToDelete);

            if (model.Lessons != null)
            {
                for (int i = 0; i < model.Lessons.Count; i++)
                {
                    var lessonModel = model.Lessons[i];
                    lessonModel.Order = i + 1; // Re-order
                    if (lessonModel.Id == 0) course.Lessons.Add(lessonModel);
                    else
                    {
                        var existingLesson = course.Lessons.FirstOrDefault(l => l.Id == lessonModel.Id);
                        if (existingLesson != null)
                        {
                            existingLesson.Title = lessonModel.Title;
                            existingLesson.BunnyLibraryId = lessonModel.BunnyLibraryId;
                            existingLesson.BunnyVideoId = lessonModel.BunnyVideoId;
                            existingLesson.Order = lessonModel.Order;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Course updated successfully!";
            return RedirectToAction(nameof(Index));
        }


        #region API Endpoints

        [HttpGet("companyadmin/api/stats")]
        public async Task<IActionResult> GetStats()
        {
            var user = await GetCurrentUserAsync();
            if (user?.CompanyId == null) return Unauthorized();

            var stats = new
            {
                totalProducts = await _context.Products.CountAsync(p => p.CompanyId == user.CompanyId),
                totalCourses = await _context.Courses.CountAsync(c => c.CompanyId == user.CompanyId),
                totalOrders = await _context.Orders.CountAsync(o => o.OrderDetails.Any(od => (od.Product != null && od.Product.CompanyId == user.CompanyId) || (od.Course != null && od.Course.CompanyId == user.CompanyId)))
            };
            return Ok(stats);
        }

        [HttpGet("companyadmin/api/products")]
        public async Task<IActionResult> GetProducts(string searchTerm)
        {
            var user = await GetCurrentUserAsync();
            if (user?.CompanyId == null) return Unauthorized();
            var query = _context.Products
                .Where(p => p.CompanyId == user.CompanyId)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm));
            }

            var products = await query
                .Select(p => new { p.Id, p.Name, p.Price, p.StockQuantity, p.ImageUrl, CategoryName = p.Category.Name })
                .ToListAsync();
            return Ok(products);
        }

        [HttpGet("companyadmin/api/product/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user?.CompanyId == null) return Unauthorized();

            var product = await _context.Products
                .Where(p => p.Id == id && p.CompanyId == user.CompanyId)
                .Select(p => new {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.SalePrice,
                    p.StockQuantity,
                    p.CategoryId,
                    p.ImageUrl
                })
                .FirstOrDefaultAsync();

            if (product == null) return NotFound();

            return Ok(product);
        }


        [HttpGet("companyadmin/api/courses")]
        public async Task<IActionResult> GetCourses(string searchTerm)
        {
            var user = await GetCurrentUserAsync();
            if (user?.CompanyId == null) return Unauthorized();

            var query = _context.Courses
                .Where(c => c.CompanyId == user.CompanyId)
                .Include(c => c.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm) || c.InstructorName.Contains(searchTerm));
            }

            var courses = await query
                .Select(c => new { c.Id, c.Name, c.Price, c.InstructorName, c.ImageUrl, CategoryName = c.Category.Name })
                .ToListAsync();
            return Ok(courses);
        }

        [HttpGet("companyadmin/api/course/{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user?.CompanyId == null) return Unauthorized();

            var course = await _context.Courses
                .Where(c => c.Id == id && c.CompanyId == user.CompanyId)
                .Select(c => new {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.Price,
                    c.SalePrice,
                    c.InstructorName,
                    c.CategoryId,
                    c.ImageUrl
                })
                .FirstOrDefaultAsync();

            if (course == null) return NotFound();

            return Ok(course);
        }

        [HttpPost("companyadmin/api/product")]
        public async Task<IActionResult> SaveProduct([FromForm] Product model)
        {
            var user = await GetCurrentUserAsync();
            if (user?.CompanyId == null) return Unauthorized();
            model.CompanyId = user.CompanyId.Value;

            ModelState.Remove("Company");
            ModelState.Remove("Category");
            ModelState.Remove("Reviews");
            ModelState.Remove("ImageUrl");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (model.Id == 0) // New Product
            {
                if (model.ImageFile != null)
                {
                    model.ImageUrl = await SaveFileAsync(model.ImageFile, "products");
                }
                _context.Products.Add(model);
            }
            else // Existing Product
            {
                var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == model.Id && p.CompanyId == user.CompanyId);
                if (existingProduct == null) return NotFound();

                if (model.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        DeleteFile(existingProduct.ImageUrl);
                    }
                    model.ImageUrl = await SaveFileAsync(model.ImageFile, "products");
                }
                else
                {
                    model.ImageUrl = existingProduct.ImageUrl; // Keep old image if no new one is uploaded
                }
                _context.Products.Update(model);
            }
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("companyadmin/api/course")]
        public async Task<IActionResult> SaveCourse([FromForm] Course model)
        {
            var user = await GetCurrentUserAsync();
            if (user?.CompanyId == null) return Unauthorized();
            model.CompanyId = user.CompanyId.Value;

            ModelState.Remove("Company");
            ModelState.Remove("Category");
            ModelState.Remove("Lessons");
            ModelState.Remove("Reviews");
            ModelState.Remove("ImageUrl");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            bool isNew = model.Id == 0;
            if (isNew) // New Course
            {
                if (model.ImageFile != null)
                {
                    model.ImageUrl = await SaveFileAsync(model.ImageFile, "courses");
                }
                _context.Courses.Add(model);
            }
            else // Existing Course
            {
                var existingCourse = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == model.Id && c.CompanyId == user.CompanyId);
                if (existingCourse == null) return NotFound();

                if (model.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingCourse.ImageUrl))
                    {
                        DeleteFile(existingCourse.ImageUrl);
                    }
                    model.ImageUrl = await SaveFileAsync(model.ImageFile, "courses");
                }
                else
                {
                    model.ImageUrl = existingCourse.ImageUrl; // Keep old image
                }
                _context.Courses.Update(model);
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, newCourseId = isNew ? model.Id : 0 });
        }

        [HttpDelete("companyadmin/api/product/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var user = await GetCurrentUserAsync();
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == user.CompanyId);
            if (product == null) return NotFound();

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                DeleteFile(product.ImageUrl);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("companyadmin/api/course/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var user = await GetCurrentUserAsync();
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == user.CompanyId);
            if (course == null) return NotFound();

            if (!string.IsNullOrEmpty(course.ImageUrl))
            {
                DeleteFile(course.ImageUrl);
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("companyadmin/api/categories")]
        public async Task<IActionResult> GetCategories(string searchTerm)
        {
            var query = _context.Categories.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm)) query = query.Where(c => c.Name.Contains(searchTerm));
            return Ok(await query.ToListAsync());
        }

        [HttpPost("companyadmin/api/category")]
        public async Task<IActionResult> SaveCategory([FromBody] Category model)
        {
            if (!ModelState.IsValid || model.Id != 0) return BadRequest();
            _context.Categories.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpGet("companyadmin/api/shippingzones")]
        public async Task<IActionResult> GetShippingZones(string searchTerm)
        {
            var query = _context.ShippingZones.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm)) query = query.Where(z => z.ZoneName.Contains(searchTerm) || z.City.Contains(searchTerm));
            return Ok(await query.ToListAsync());
        }

        [HttpGet("companyadmin/api/shippingzone/{id}")]
        public async Task<IActionResult> GetShippingZone(int id)
        {
            if (id == 0) return Ok(new ShippingZone());
            var zone = await _context.ShippingZones.FindAsync(id);
            if (zone == null) return NotFound();
            return Ok(zone);
        }

        [HttpPost("companyadmin/api/shippingzone")]
        public async Task<IActionResult> SaveShippingZone([FromBody] ShippingZone model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (model.Id == 0) _context.ShippingZones.Add(model);
            else _context.ShippingZones.Update(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpDelete("companyadmin/api/shippingzone/{id}")]
        public async Task<IActionResult> DeleteShippingZone(int id)
        {
            var zone = await _context.ShippingZones.FindAsync(id);
            if (zone == null) return NotFound();
            _context.ShippingZones.Remove(zone);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("companyadmin/api/travelpackages")]
        public async Task<IActionResult> GetTravelPackages(string searchTerm)
        {
            var user = await GetCurrentUserAsync();
            if (user?.Company?.Name != "Mithaqq Easy Way") return Forbid();

            var query = _context.TravelPackages.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm)) query = query.Where(p => p.Name.Contains(searchTerm) || p.Destination.Contains(searchTerm));
            return Ok(await query.ToListAsync());
        }

        [HttpGet("companyadmin/api/travelpackage/{id}")]
        public async Task<IActionResult> GetTravelPackage(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user?.Company?.Name != "Mithaqq Easy Way") return Forbid();

            var package = await _context.TravelPackages
                .Where(p => p.Id == id && p.CompanyId == user.CompanyId)
                .Select(p => new {
                    p.Id,
                    p.Name,
                    p.Destination,
                    p.Description,
                    p.Price,
                    p.DurationDays,
                    p.Inclusions,
                    p.ImageUrl
                })
                .FirstOrDefaultAsync();

            if (package == null) return NotFound();

            return Ok(package);
        }

        [HttpPost("companyadmin/api/travelpackage")]
        public async Task<IActionResult> SaveTravelPackage([FromForm] TravelPackage model)
        {
            var user = await GetCurrentUserAsync();
            if (user?.Company?.Name != "Mithaqq Easy Way") return Forbid();

            model.CompanyId = user.CompanyId.Value;

            ModelState.Remove("Company");
            ModelState.Remove("ImageUrl");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (model.Id == 0) // New Package
            {
                if (model.ImageFile != null)
                {
                    model.ImageUrl = await SaveFileAsync(model.ImageFile, "travel");
                }
                _context.TravelPackages.Add(model);
            }
            else // Existing Package
            {
                var existingPackage = await _context.TravelPackages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == model.Id && p.CompanyId == user.CompanyId);
                if (existingPackage == null) return NotFound();

                if (model.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingPackage.ImageUrl))
                    {
                        DeleteFile(existingPackage.ImageUrl);
                    }
                    model.ImageUrl = await SaveFileAsync(model.ImageFile, "travel");
                }
                else
                {
                    model.ImageUrl = existingPackage.ImageUrl;
                }
                _context.TravelPackages.Update(model);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpDelete("companyadmin/api/travelpackage/{id}")]
        public async Task<IActionResult> DeleteTravelPackage(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user?.Company?.Name != "Mithaqq Easy Way") return Forbid();

            var package = await _context.TravelPackages.FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == user.CompanyId);
            if (package == null) return NotFound();

            if (!string.IsNullOrEmpty(package.ImageUrl))
            {
                DeleteFile(package.ImageUrl);
            }

            _context.TravelPackages.Remove(package);
            await _context.SaveChangesAsync();
            return Ok();
        }

        #endregion

        #region Helper Methods
        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images", folderName);
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsDir, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return $"/images/{folderName}/{fileName}";
        }

        private void DeleteFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
        }
        #endregion
    }
}

