using System.ComponentModel.DataAnnotations;

namespace Mithaqq.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "The Password field is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
