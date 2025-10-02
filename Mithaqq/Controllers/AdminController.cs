using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mithaqq.Data;
using Mithaqq.Models;
using Mithaqq.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace Mithaqq.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Companies"] = await _context.Companies.ToListAsync();
            ViewData["Categories"] = await _context.Categories.ToListAsync();
            ViewData["Roles"] = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View();
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserType = user.UserType,
                CompanyId = user.CompanyId,
                SelectedRoles = userRoles,
                AllCompanies = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name"),
                AllRoles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name"),
                CommissionRate = user.CommissionRate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllCompanies = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name");
                model.AllRoles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.UserType = model.UserType;
            user.CompanyId = model.CompanyId;
            user.CommissionRate = model.CommissionRate;


            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var newRoles = model.SelectedRoles ?? new List<string>();

                var rolesToRemove = currentRoles.Except(newRoles);
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                var rolesToAdd = newRoles.Except(currentRoles);
                await _userManager.AddToRolesAsync(user, rolesToAdd);

                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.AllCompanies = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name");
            model.AllRoles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses.Include(c => c.Lessons).FirstOrDefaultAsync(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }

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
                AllCompanies = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name", course.CompanyId),
                AllCategories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", course.CategoryId)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(EditCourseViewModel model)
        {
            try
            {
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
                    model.AllCompanies = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name", model.CompanyId);
                    model.AllCategories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
                    return View(model);
                }

                var course = await _context.Courses.Include(c => c.Lessons).FirstOrDefaultAsync(c => c.Id == model.Id);
                if (course == null)
                {
                    return NotFound();
                }

                course.Name = model.Name;
                course.Description = model.Description;
                course.Price = model.Price;
                course.SalePrice = model.SalePrice;
                course.InstructorName = model.InstructorName;
                course.IsOnline = course.IsOnline;
                course.CompanyId = model.CompanyId;
                course.CategoryId = model.CategoryId;

                var incomingLessonIds = model.Lessons?.Select(l => l.Id).ToList() ?? new List<int>();
                var lessonsToDelete = course.Lessons.Where(dbLesson => !incomingLessonIds.Contains(dbLesson.Id)).ToList();
                _context.Lessons.RemoveRange(lessonsToDelete);

                if (model.Lessons != null)
                {
                    for (int i = 0; i < model.Lessons.Count; i++)
                    {
                        var lessonModel = model.Lessons[i];
                        if (lessonModel.Id == 0) // New lesson
                        {
                            var newLesson = new Lesson
                            {
                                Title = lessonModel.Title,
                                BunnyLibraryId = lessonModel.BunnyLibraryId,
                                BunnyVideoId = lessonModel.BunnyVideoId,
                                Order = i + 1
                            };
                            course.Lessons.Add(newLesson);
                        }
                        else // Existing lesson
                        {
                            var existingLesson = course.Lessons.FirstOrDefault(l => l.Id == lessonModel.Id);
                            if (existingLesson != null)
                            {
                                existingLesson.Title = lessonModel.Title;
                                existingLesson.BunnyLibraryId = lessonModel.BunnyLibraryId;
                                existingLesson.BunnyVideoId = lessonModel.BunnyVideoId;
                                existingLesson.Order = i + 1;
                            }
                        }
                    }
                }

                _context.Update(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Course and lessons updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                string errorMessage = "An unexpected error occurred while saving the course.";
                if (ex is ArgumentOutOfRangeException)
                {
                    errorMessage = "A data binding error occurred. Please ensure all lesson fields are filled correctly and try again.";
                }
                TempData["ErrorMessage"] = errorMessage + " Your changes have not been saved. Please review the data and try again.";
                model.AllCompanies = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name", model.CompanyId);
                model.AllCategories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }
        }


        #region API Methods

        [HttpGet("admin/api/stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = new
            {
                totalProducts = await _context.Products.CountAsync(),
                totalOrders = await _context.Orders.CountAsync(),
                totalMarketers = await _userManager.GetUsersInRoleAsync("Marketer").ContinueWith(t => t.Result.Count),
                monthlyRevenue = await _context.Orders
                    .Where(o => o.OrderDate > DateTime.Now.AddMonths(-1) && o.Status == "Completed")
                    .SumAsync(o => o.OrderTotal),
                totalCourses = await _context.Courses.CountAsync(),
                totalStudents = await _userManager.GetUsersInRoleAsync("User").ContinueWith(t => t.Result.Count)
            };
            return Ok(stats);
        }

        [HttpGet("admin/api/analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var salesData = await _context.Orders
                .Where(o => o.Status == "Completed" && o.OrderDate >= sixMonthsAgo)
                .Select(o => new { o.OrderDate, o.OrderTotal })
                .ToListAsync();

            var monthlySales = salesData
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new ChartData
                {
                    Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    Value = g.Sum(o => o.OrderTotal)
                })
                .OrderBy(d => d.Label)
                .ToList();

            if (!monthlySales.Any())
            {
                monthlySales = new List<ChartData>
                {
                    new ChartData { Label = DateTime.Now.AddMonths(-5).ToString("MMM yyyy"), Value = 12000 },
                    new ChartData { Label = DateTime.Now.AddMonths(-4).ToString("MMM yyyy"), Value = 19000 },
                    new ChartData { Label = DateTime.Now.AddMonths(-3).ToString("MMM yyyy"), Value = 15000 },
                    new ChartData { Label = DateTime.Now.AddMonths(-2).ToString("MMM yyyy"), Value = 25000 },
                    new ChartData { Label = DateTime.Now.AddMonths(-1).ToString("MMM yyyy"), Value = 22000 },
                    new ChartData { Label = DateTime.Now.ToString("MMM yyyy"), Value = 30000 }
                };
            }

            var productSalesByCategory = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                    .ThenInclude(p => p.Category)
                .Where(od => od.Order.Status == "Completed" && od.ProductId != null && od.Product.Category != null)
                .GroupBy(od => od.Product.Category.Name)
                .Select(g => new { CategoryName = g.Key, Total = g.Sum(od => od.UnitPrice * od.Quantity) })
                .ToListAsync();

            var courseSalesByCategory = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Course)
                    .ThenInclude(c => c.Category)
                .Where(od => od.Order.Status == "Completed" && od.CourseId != null && od.Course.Category != null)
                .GroupBy(od => od.Course.Category.Name)
                .Select(g => new { CategoryName = g.Key, Total = g.Sum(od => od.UnitPrice * od.Quantity) })
                .ToListAsync();

            var allSales = productSalesByCategory.Concat(courseSalesByCategory);

            var salesByCategory = allSales
                .GroupBy(s => s.CategoryName)
                .Select(g => new ChartData
                {
                    Label = g.Key,
                    Value = g.Sum(s => s.Total)
                })
                .ToList();

            if (!salesByCategory.Any())
            {
                salesByCategory = new List<ChartData>
                {
                    new ChartData { Label = "Service Packages", Value = 3500 },
                    new ChartData { Label = "Training Courses", Value = 3000 },
                    new ChartData { Label = "Tech Solutions", Value = 2000 },
                    new ChartData { Label = "Travel Services", Value = 1500 }
                };
            }


            var topProducts = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Where(od => od.ProductId.HasValue && od.Order.Status == "Completed" && od.Product != null)
                .GroupBy(od => od.Product.Name)
                .Select(g => new TopItemData
                {
                    Name = g.Key,
                    Quantity = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(p => p.Quantity)
                .Take(5)
                .ToListAsync();

            var topCourses = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Course)
                .Where(od => od.CourseId.HasValue && (od.Order.Status == "Completed" || od.Order.Status == "Pending Approval") && od.Course != null)
                .GroupBy(od => od.Course.Name)
                .Select(g => new TopItemData
                {
                    Name = g.Key,
                    Quantity = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(c => c.Quantity)
                .Take(5)
                .ToListAsync();

            var enrollmentData = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.CourseId.HasValue && (od.Order.Status == "Completed" || od.Order.Status == "Pending Approval") && od.Order.OrderDate >= sixMonthsAgo)
                .Select(od => new { od.Order.OrderDate, od.Quantity })
                .ToListAsync();

            var monthlyEnrollments = enrollmentData
                .GroupBy(od => new { od.OrderDate.Year, od.OrderDate.Month })
                .Select(g => new ChartData
                {
                    Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    Value = g.Sum(od => od.Quantity)
                })
                .OrderBy(d => d.Label)
                .ToList();

            if (!monthlyEnrollments.Any())
            {
                monthlyEnrollments = new List<ChartData>
                {
                    new ChartData { Label = DateTime.Now.AddMonths(-5).ToString("MMM yyyy"), Value = 45 },
                    new ChartData { Label = DateTime.Now.AddMonths(-4).ToString("MMM yyyy"), Value = 67 },
                    new ChartData { Label = DateTime.Now.AddMonths(-3).ToString("MMM yyyy"), Value = 89 },
                    new ChartData { Label = DateTime.Now.AddMonths(-2).ToString("MMM yyyy"), Value = 123 },
                    new ChartData { Label = DateTime.Now.AddMonths(-1).ToString("MMM yyyy"), Value = 156 },
                    new ChartData { Label = DateTime.Now.ToString("MMM yyyy"), Value = 189 }
                };
            }

            // Static data for completion rates as there's no model for it.
            var completionRates = new List<ChartData>
            {
                new ChartData { Label = "Completed", Value = 65 },
                new ChartData { Label = "In Progress", Value = 25 },
                new ChartData { Label = "Not Started", Value = 10 }
            };

            var analytics = new AnalyticsViewModel
            {
                MonthlySales = monthlySales,
                SalesByCategory = salesByCategory,
                TopProducts = topProducts,
                TopCourses = topCourses,
                MonthlyEnrollments = monthlyEnrollments,
                CompletionRates = completionRates
            };
            return Ok(analytics);
        }


        [HttpGet("admin/api/reviews")]
        public async Task<IActionResult> GetReviews(string searchTerm, string itemType)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.Course)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r =>
                    (r.Product != null && r.Product.Name.Contains(searchTerm)) ||
                    (r.Course != null && r.Course.Name.Contains(searchTerm)) ||
                    (r.User.FirstName + " " + r.User.LastName).Contains(searchTerm) ||
                    (r.Comment != null && r.Comment.Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(itemType) && itemType != "All")
            {
                if (itemType == "Product")
                {
                    query = query.Where(r => r.ProductId.HasValue);
                }
                else if (itemType == "Course")
                {
                    query = query.Where(r => r.CourseId.HasValue);
                }
            }


            var reviews = await query
                .OrderByDescending(r => r.DatePosted)
                .Select(r => new ReviewAdminViewModel
                {
                    Id = r.Id,
                    ItemName = r.Product != null ? r.Product.Name : r.Course.Name,
                    ItemType = r.ProductId.HasValue ? "Product" : "Course",
                    UserName = r.User.FirstName + " " + r.User.LastName,
                    Stars = r.Stars,
                    Comment = r.Comment,
                    DatePosted = r.DatePosted
                })
                .ToListAsync();
            return Ok(reviews);
        }

        [HttpDelete("admin/api/review/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            var productId = review.ProductId;
            var courseId = review.CourseId;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            await UpdateAverageRating(productId, courseId);

            return Ok(new { success = true });
        }


        [HttpGet("admin/api/users")]
        public async Task<IActionResult> GetUsers(string searchTerm)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => (u.FirstName + " " + u.LastName).Contains(searchTerm) || u.Email.Contains(searchTerm));
            }

            var users = await query.ToListAsync();
            var userList = new List<object>();
            foreach (var user in users)
            {
                userList.Add(new
                {
                    id = user.Id,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    roles = await _userManager.GetRolesAsync(user)
                });
            }
            return Ok(userList);
        }


        [HttpGet("admin/api/user/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                userType = user.UserType,
                companyId = user.CompanyId,
                selectedRoles = await _userManager.GetRolesAsync(user),
                commissionRate = user.CommissionRate
            };
            return Ok(model);
        }

        [HttpPost("admin/api/user")]
        public async Task<IActionResult> SaveUser([FromBody] EditUserViewModel model)
        {
            ModelState.Remove("AllCompanies");
            ModelState.Remove("AllRoles");

            if (!ModelState.IsValid)
            {
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            if (string.IsNullOrEmpty(model.Id))
            {
                if (string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { errors = new[] { "Password is required for new users." } });
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserType = model.UserType,
                    CompanyId = model.CompanyId,
                    EmailConfirmed = true,
                    CommissionRate = model.CommissionRate
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }
                    return Ok(new { success = true });
                }
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }
            else
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.UserType = model.UserType;
                user.CompanyId = model.CompanyId;
                user.CommissionRate = model.CommissionRate;


                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var newRoles = model.SelectedRoles ?? new List<string>();

                    await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(newRoles));
                    await _userManager.AddToRolesAsync(user, newRoles.Except(currentRoles));

                    return Ok(new { success = true });
                }
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }
        }

        [HttpDelete("admin/api/user/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { success = true });
            }
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        [HttpGet("admin/api/products")]
        public async Task<IActionResult> GetProducts(string searchTerm, int? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Company)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    CompanyName = p.Company.Name,
                    CategoryName = p.Category.Name,
                    p.Price,
                    p.StockQuantity,
                    p.ImageUrl
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("api/products/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            if (id == 0) return Ok(new Product());
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost("admin/api/product")]
        public async Task<IActionResult> SaveProduct([FromForm] Product model)
        {
            ModelState.Remove("Company");
            ModelState.Remove("Category");
            ModelState.Remove("ImageFile");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Reviews");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Validation failed", errors });
            }

            if (model.Price > 0 && model.SalePrice.HasValue && model.SalePrice > 0)
            {
                model.DiscountPercentage = (int)Math.Round(((model.Price - model.SalePrice.Value) / model.Price) * 100);
            }
            else
            {
                model.DiscountPercentage = 0;
            }

            if (model.Id == 0)
            {
                if (model.ImageFile != null)
                {
                    model.ImageUrl = await SaveFileAsync(model.ImageFile, "products");
                }
                _context.Products.Add(model);
            }
            else
            {
                var existingProduct = await _context.Products.FindAsync(model.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.Name = model.Name;
                existingProduct.Description = model.Description;
                existingProduct.Price = model.Price;
                existingProduct.SalePrice = model.SalePrice;
                existingProduct.DiscountPercentage = model.DiscountPercentage;
                existingProduct.StockQuantity = model.StockQuantity;
                existingProduct.CompanyId = model.CompanyId;
                existingProduct.CategoryId = model.CategoryId;

                if (model.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        DeleteFile(existingProduct.ImageUrl);
                    }
                    existingProduct.ImageUrl = await SaveFileAsync(model.ImageFile, "products");
                }
                _context.Products.Update(existingProduct);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }


        [HttpDelete("admin/api/product/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                DeleteFile(product.ImageUrl);
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("admin/api/courses")]
        public async Task<IActionResult> GetCourses(string searchTerm, int? categoryId, int? companyId)
        {
            var query = _context.Courses
                .Include(c => c.Company)
                .Include(c => c.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm) || c.InstructorName.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            if (companyId.HasValue)
            {
                query = query.Where(c => c.CompanyId == companyId.Value);
            }

            var courses = await query
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    CompanyName = c.Company.Name,
                    CategoryName = c.Category.Name,
                    c.Price,
                    c.InstructorName,
                    c.ImageUrl
                })
                .ToListAsync();
            return Ok(courses);
        }

        [HttpGet("api/courses/{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            if (id == 0) return Ok(new Course());
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            return Ok(course);
        }

        [HttpPost("admin/api/course")]
        public async Task<IActionResult> SaveCourse([FromForm] Course model)
        {
            ModelState.Remove("Company");
            ModelState.Remove("Category");
            ModelState.Remove("ImageFile");
            ModelState.Remove("Lessons");
            ModelState.Remove("Reviews");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            bool isNew = model.Id == 0;

            if (isNew)
            {
                if (model.ImageFile != null)
                {
                    model.ImageUrl = await SaveFileAsync(model.ImageFile, "courses");
                }
                _context.Courses.Add(model);
            }
            else
            {
                var existingCourse = await _context.Courses.FindAsync(model.Id);
                if (existingCourse == null) return NotFound();

                existingCourse.Name = model.Name;
                existingCourse.Description = model.Description;
                existingCourse.Price = model.Price;
                existingCourse.SalePrice = model.SalePrice;
                existingCourse.InstructorName = model.InstructorName;
                existingCourse.CompanyId = model.CompanyId;
                existingCourse.CategoryId = model.CategoryId;

                if (model.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingCourse.ImageUrl))
                    {
                        DeleteFile(existingCourse.ImageUrl);
                    }
                    existingCourse.ImageUrl = await SaveFileAsync(model.ImageFile, "courses");
                }
                _context.Courses.Update(existingCourse);
            }
            await _context.SaveChangesAsync();

            if (isNew)
            {
                return Ok(new { success = true, newCourseId = model.Id });
            }

            return Ok(new { success = true });
        }

        [HttpDelete("admin/api/course/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            if (!string.IsNullOrEmpty(course.ImageUrl))
            {
                DeleteFile(course.ImageUrl);
            }
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("admin/api/categories")]
        public async Task<IActionResult> GetCategories(string searchTerm)
        {
            var query = _context.Categories.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm));
            }
            return Ok(await query.ToListAsync());
        }

        [HttpGet("admin/api/category/{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            if (id == 0) return Ok(new Category());
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpPost("admin/api/category")]
        public async Task<IActionResult> SaveCategory([FromBody] Category model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (model.Id == 0) _context.Categories.Add(model);
            else _context.Categories.Update(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpDelete("admin/api/category/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("admin/api/shippingzones")]
        public async Task<IActionResult> GetShippingZones(string searchTerm)
        {
            var query = _context.ShippingZones.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(z => z.ZoneName.Contains(searchTerm) || z.City.Contains(searchTerm));
            }
            return Ok(await query.ToListAsync());
        }

        [HttpGet("admin/api/shippingzone/{id}")]
        public async Task<IActionResult> GetShippingZone(int id)
        {
            if (id == 0) return Ok(new ShippingZone());
            var zone = await _context.ShippingZones.FindAsync(id);
            if (zone == null) return NotFound();
            return Ok(zone);
        }

        [HttpPost("admin/api/shippingzone")]
        public async Task<IActionResult> SaveShippingZone([FromBody] ShippingZone model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (model.Id == 0) _context.ShippingZones.Add(model);
            else _context.ShippingZones.Update(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpDelete("admin/api/shippingzone/{id}")]
        public async Task<IActionResult> DeleteShippingZone(int id)
        {
            var zone = await _context.ShippingZones.FindAsync(id);
            if (zone == null) return NotFound();
            _context.ShippingZones.Remove(zone);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("admin/api/travelpackages")]
        public async Task<IActionResult> GetTravelPackages(string searchTerm)
        {
            var query = _context.TravelPackages.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Destination.Contains(searchTerm));
            }

            return Ok(await query.ToListAsync());
        }

        [HttpGet("admin/api/travelpackage/{id}")]
        public async Task<IActionResult> GetTravelPackage(int id)
        {
            if (id == 0) return Ok(new TravelPackage());
            var package = await _context.TravelPackages.FindAsync(id);
            if (package == null) return NotFound();
            return Ok(package);
        }

        [HttpPost("admin/api/travelpackage")]
        public async Task<IActionResult> SaveTravelPackage([FromForm] TravelPackage model)
        {
            ModelState.Remove("Company");
            ModelState.Remove("ImageFile");
            ModelState.Remove("ImageUrl");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (model.Id == 0)
            {
                if (model.ImageFile != null)
                {
                    model.ImageUrl = await SaveFileAsync(model.ImageFile, "travel");
                }
                _context.TravelPackages.Add(model);
            }
            else
            {
                var existingPackage = await _context.TravelPackages.FindAsync(model.Id);
                if (existingPackage == null) return NotFound();

                existingPackage.Name = model.Name;
                existingPackage.Destination = model.Destination;
                existingPackage.Description = model.Description;
                existingPackage.Price = model.Price;
                existingPackage.DurationDays = model.DurationDays;
                existingPackage.Inclusions = model.Inclusions;

                if (model.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingPackage.ImageUrl))
                    {
                        DeleteFile(existingPackage.ImageUrl);
                    }
                    existingPackage.ImageUrl = await SaveFileAsync(model.ImageFile, "travel");
                }
                _context.TravelPackages.Update(existingPackage);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
        [HttpDelete("admin/api/travelpackage/{id}")]
        public async Task<IActionResult> DeleteTravelPackage(int id)
        {
            var package = await _context.TravelPackages.FindAsync(id);
            if (package == null) return NotFound();

            if (!string.IsNullOrEmpty(package.ImageUrl))
            {
                DeleteFile(package.ImageUrl);
            }

            _context.TravelPackages.Remove(package);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("admin/api/blogposts")]
        public async Task<IActionResult> GetBlogPosts(string searchTerm)
        {
            var query = _context.BlogPosts.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchTerm) || p.AuthorName.Contains(searchTerm));
            }

            return Ok(await query.OrderByDescending(p => p.PublishDate).ToListAsync());
        }

        [HttpGet("admin/api/blogpost/{id}")]
        public async Task<IActionResult> GetBlogPost(int id)
        {
            if (id == 0) return Ok(new BlogPost());
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();
            return Ok(post);
        }

        [HttpPost("admin/api/blogpost")]
        public async Task<IActionResult> SaveBlogPost([FromForm] BlogPost model)
        {
            ModelState.Remove("ImageFile");
            ModelState.Remove("AuthorImageFile");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("AuthorImageUrl");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (model.Id == 0)
            {
                model.PublishDate = DateTime.UtcNow;
                if (model.ImageFile != null)
                {
                    model.ImageUrl = await SaveFileAsync(model.ImageFile, "blogs");
                }
                if (model.AuthorImageFile != null)
                {
                    model.AuthorImageUrl = await SaveFileAsync(model.AuthorImageFile, "authors");
                }
                _context.BlogPosts.Add(model);
            }
            else
            {
                var existingPost = await _context.BlogPosts.FindAsync(model.Id);
                if (existingPost == null) return NotFound();

                existingPost.Title = model.Title;
                existingPost.Subtitle = model.Subtitle;
                existingPost.Content = model.Content;
                existingPost.AuthorName = model.AuthorName;
                existingPost.AuthorTitle = model.AuthorTitle;

                if (model.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingPost.ImageUrl))
                    {
                        DeleteFile(existingPost.ImageUrl);
                    }
                    existingPost.ImageUrl = await SaveFileAsync(model.ImageFile, "blogs");
                }
                if (model.AuthorImageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingPost.AuthorImageUrl))
                    {
                        DeleteFile(existingPost.AuthorImageUrl);
                    }
                    existingPost.AuthorImageUrl = await SaveFileAsync(model.AuthorImageFile, "authors");
                }
                _context.BlogPosts.Update(existingPost);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpDelete("admin/api/blogpost/{id}")]
        public async Task<IActionResult> DeleteBlogPost(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();

            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                DeleteFile(post.ImageUrl);
            }
            if (!string.IsNullOrEmpty(post.AuthorImageUrl))
            {
                DeleteFile(post.AuthorImageUrl);
            }

            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("admin/api/orders")]
        public async Task<IActionResult> GetOrders(string searchTerm, string status)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(o => (o.User.FirstName + " " + o.User.LastName).Contains(searchTerm) || o.Id.ToString().Contains(searchTerm));
            }
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                query = query.Where(o => o.Status == status);
            }


            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    CustomerName = o.User.FirstName + " " + o.User.LastName,
                    OrderDate = o.OrderDate,
                    OrderTotal = o.OrderTotal,
                    Status = o.Status
                })
                .ToListAsync();
            return Ok(orders);
        }

        [HttpGet("admin/api/order/{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Course)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.TravelPackage)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var viewModel = new OrderDetailViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                CustomerName = order.User.FirstName + " " + order.User.LastName,
                CustomerEmail = order.User.Email,
                PhoneNumber = order.PhoneNumber,
                ShippingAddress = order.ShippingAddress,
                OrderTotal = order.OrderTotal,
                PaymentMethod = order.PaymentMethod,
                PaymentId = order.PaymentId,
                Items = order.OrderDetails.Select(od => new OrderItemViewModel
                {
                    ItemName = od.Product?.Name ?? od.Course?.Name ?? od.TravelPackage?.Name,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice
                }).ToList()
            };
            return Ok(viewModel);
        }


        [HttpPost("admin/api/order/updatestatus")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusModel model)
        {
            var order = await _context.Orders.FindAsync(model.OrderId);
            if (order == null) return NotFound();

            order.Status = model.Status;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost("admin/api/deletelesson/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound(new { success = false, message = "Lesson not found." });
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Lesson deleted successfully." });
        }


        #endregion

        #region Helper Methods
        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images", folderName);
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }
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
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
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
        #endregion
    }
}