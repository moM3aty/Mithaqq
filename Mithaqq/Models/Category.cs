using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Mithaqq.Models
{
    /// <summary>
    /// Represents a category for products or courses.
    /// The 'Type' property has been removed as it's redundant.
    /// </summary>
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
