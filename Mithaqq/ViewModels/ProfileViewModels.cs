using Mithaqq.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mithaqq.ViewModels
{
    public class ProfileViewModel
    {
        public ApplicationUser User { get; set; }
        public IEnumerable<Order> Orders { get; set; }
        public IEnumerable<Course> Courses { get; set; }
        public EditProfileViewModel EditProfileViewModel { get; set; }
        public ChangePasswordViewModel ChangePasswordViewModel { get; set; }
    }

    public class EditProfileViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string Address { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
