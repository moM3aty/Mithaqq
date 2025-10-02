using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mithaqq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mithaqq.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // This will apply pending migrations and create the database if it doesn't exist.
            await context.Database.MigrateAsync();

            // --- Seed Roles ---
            if (!await roleManager.Roles.AnyAsync())
            {
                string[] roleNames = { "Admin", "CompanyAdmin", "Marketer", "User" };
                foreach (var roleName in roleNames)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // --- Seed Companies ---
            if (!await context.Companies.AnyAsync())
            {
                var companies = new List<Company>
                {
                    new Company { Name = "Mithaqq Global", Description = "Training and Development" },
                    new Company { Name = "Mithaqq Nile", Description = "E-Marketing Solutions" },
                    new Company { Name = "Mithaqq Innovation", Description = "Technical Solutions" },
                    new Company { Name = "Mithaqq Easy Way", Description = "Travel and Tourism" }
                };
                await context.Companies.AddRangeAsync(companies);
                await context.SaveChangesAsync();
            }

            // --- Seed Users ---
            if (!await userManager.Users.AnyAsync())
            {
                var globalCompanyId = (await context.Companies.FirstOrDefaultAsync(c => c.Name == "Mithaqq Global"))?.Id;

                var adminUser = new ApplicationUser { UserName = "admin@mithaqq.com", Email = "admin@mithaqq.com", FirstName = "Super", LastName = "Admin", EmailConfirmed = true, UserType = "Admin" };
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, "Admin");

                if (globalCompanyId.HasValue)
                {
                    var companyAdminUser = new ApplicationUser { UserName = "companyadmin@mithaqq.com", Email = "companyadmin@mithaqq.com", FirstName = "Company", LastName = "Admin", EmailConfirmed = true, UserType = "CompanyAdmin", CompanyId = globalCompanyId.Value };
                    await userManager.CreateAsync(companyAdminUser, "Admin@123");
                    await userManager.AddToRoleAsync(companyAdminUser, "CompanyAdmin");
                }

                var marketerUser = new ApplicationUser { UserName = "marketer@mithaqq.com", Email = "marketer@mithaqq.com", FirstName = "Active", LastName = "Marketer", EmailConfirmed = true, UserType = "Marketer", ReferralCode = "MARKET123" };
                await userManager.CreateAsync(marketerUser, "Admin@123");
                await userManager.AddToRoleAsync(marketerUser, "Marketer");

                var regularUser = new ApplicationUser { UserName = "user@mithaqq.com", Email = "user@mithaqq.com", FirstName = "Normal", LastName = "User", EmailConfirmed = true, UserType = "Normal" };
                await userManager.CreateAsync(regularUser, "Admin@123");
                await userManager.AddToRoleAsync(regularUser, "User");

                var mohamedUser = new ApplicationUser { UserName = "mo.m3aty@yahoo.com", Email = "mo.m3aty@yahoo.com", FirstName = "Mohamed", LastName = "Aboelmaaty", EmailConfirmed = true, UserType = "User" };
                await userManager.CreateAsync(mohamedUser, "Admin@123");
                await userManager.AddToRoleAsync(mohamedUser, "User");
            }

            // --- Seed Categories ---
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Business & Management" }, new Category { Name = "Technology & IT" },
                    new Category { Name = "Marketing & Sales" }, new Category { Name = "Service Packages" },
                    new Category { Name = "Tech Solutions" }, new Category { Name = "Human Resources" },
                    new Category { Name = "Finance & Accounting" }, new Category { Name = "Soft Skills" },
                    new Category { Name = "Travel Services" }
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // --- Seed Products, Courses, etc. only if they don't exist ---
            if (!await context.Products.AnyAsync() && !await context.Courses.AnyAsync())
            {
                var globalCoId = (await context.Companies.FirstOrDefaultAsync(c => c.Name == "Mithaqq Global")).Id;
                var nileCoId = (await context.Companies.FirstOrDefaultAsync(c => c.Name == "Mithaqq Nile")).Id;
                var innovationCoId = (await context.Companies.FirstOrDefaultAsync(c => c.Name == "Mithaqq Innovation")).Id;
                var easyWayCoId = (await context.Companies.FirstOrDefaultAsync(c => c.Name == "Mithaqq Easy Way")).Id;

                var mgmtCatId = (await context.Categories.FirstOrDefaultAsync(c => c.Name == "Business & Management")).Id;
                var techCatId = (await context.Categories.FirstOrDefaultAsync(c => c.Name == "Technology & IT")).Id;
                var marketingCatId = (await context.Categories.FirstOrDefaultAsync(c => c.Name == "Marketing & Sales")).Id;

                // Courses
                var course1 = new Course
                {
                    Name = "Leadership & Management Skills",
                    Description = "Master essential leadership skills.",
                    Price = 350,
                    SalePrice = 299,
                    InstructorName = "Dr. Ahmed Khalid",
                    CompanyId = globalCoId,
                    CategoryId = mgmtCatId,
                    IsOnline = true,
                    ImageUrl = "/images/courses/course1.jpg",
                    Lessons = new List<Lesson> {
                        new Lesson { Title = "Introduction to Modern Leadership", Order = 1, BunnyLibraryId = 12345, BunnyVideoId = "vid-001" },
                        new Lesson { Title = "Effective Communication Strategies", Order = 2, BunnyLibraryId = 12345, BunnyVideoId = "vid-002" },
                    }
                };
                context.Courses.Add(course1);

                // Products
                var product1 = new Product { Name = "Social Media Management - Gold Tier", Description = "Comprehensive social media management...", Price = 499, SalePrice = 399, StockQuantity = 10, CompanyId = nileCoId, CategoryId = marketingCatId, ImageUrl = "/images/products/product1.jpg", DiscountPercentage = 20 };
                context.Products.Add(product1);

                // Travel
                context.TravelPackages.Add(new TravelPackage { Name = "Dubai Adventure Week", Destination = "Dubai, UAE", Description = "Experience the best of Dubai...", Price = 1500, DurationDays = 7, Inclusions = "Flights, 5-Star Hotel, Tours", CompanyId = easyWayCoId, ImageUrl = "/images/travel/dubai.jpg" });

                // Blogs
                context.BlogPosts.Add(new BlogPost { Title = "The Future of Digital Marketing", Subtitle = "Trends to watch in the coming year.", Content = "...", AuthorName = "Jane Doe", AuthorTitle = "Marketing Expert", PublishDate = DateTime.UtcNow.AddDays(-5), ImageUrl = "/images/blogs/blog1.jpg", AuthorImageUrl = "/images/authors/author1.jpg" });

                // Save changes to generate IDs for products and courses
                await context.SaveChangesAsync();

                // Now that product1 has an ID, we can seed the review
                var mohamedUser = await userManager.FindByEmailAsync("mo.m3aty@yahoo.com");
                if (mohamedUser != null)
                {
                    context.Reviews.Add(new Review { ProductId = product1.Id, UserId = mohamedUser.Id, Stars = 4, Comment = "Great package, really helped our social media presence!", DatePosted = DateTime.UtcNow });
                }

                await context.SaveChangesAsync();

                // Update average ratings
                product1.RatingCount = 1;
                product1.AverageRating = 4;
                context.Products.Update(product1);

                await context.SaveChangesAsync();
            }
        }
    }
}

