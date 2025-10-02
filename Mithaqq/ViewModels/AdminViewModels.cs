using Microsoft.AspNetCore.Mvc.Rendering;
using Mithaqq.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mithaqq.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCourses { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalOrders { get; set; }
    }
    public class DashboardStatsViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalMarketers { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "User Type")]
        public string UserType { get; set; }

        [Display(Name = "Company")]
        public int? CompanyId { get; set; }

        [Required]
        public IList<string> SelectedRoles { get; set; } = new List<string>();
        public SelectList AllCompanies { get; set; }

        public SelectList AllRoles { get; set; }

        [Display(Name = "Commission Rate")]
        [Range(0.00, 1.00, ErrorMessage = "Commission rate must be between 0.00 and 1.00 (e.g., 0.1 for 10%).")]
        public decimal CommissionRate { get; set; }

    }
    public class EditCourseViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public string ImageUrl { get; set; }
        public string InstructorName { get; set; }
        public bool IsOnline { get; set; }
        public int CompanyId { get; set; }
        public int CategoryId { get; set; }
        public List<Lesson> Lessons { get; set; }
        public SelectList AllCompanies { get; set; }
        public SelectList AllCategories { get; set; }
    }
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public System.DateTime OrderDate { get; set; }
        public decimal OrderTotal { get; set; }
        public string Status { get; set; }
    }

    public class OrderDetailViewModel
    {
        public int Id { get; set; }
        public System.DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public decimal OrderTotal { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentId { get; set; }
        public List<OrderItemViewModel> Items { get; set; }
    }

    public class OrderItemViewModel
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class UpdateOrderStatusModel
    {
        [Required]
        public int OrderId { get; set; }
        [Required]
        public string Status { get; set; }
    }

}
