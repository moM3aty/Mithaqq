using System.ComponentModel.DataAnnotations;

namespace Mithaqq.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? LogoUrl { get; set; }

        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}

