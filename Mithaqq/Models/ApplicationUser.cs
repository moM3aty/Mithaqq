using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mithaqq.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        public string? Address { get; set; }

        public override string? PhoneNumber { get; set; }

        [Required]
        public string UserType { get; set; } = "Normal";

        public int? CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public string? ReferralCode { get; set; }

        public string? ReferredBy { get; set; }

        [Column(TypeName = "decimal(5, 2)")] // Allows values like 0.10 for 10%
        [Display(Name = "Commission Rate")]
        public decimal CommissionRate { get; set; } = 0.10m; // Default to 10%
    }
}
