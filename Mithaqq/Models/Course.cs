using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mithaqq.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalePrice { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public string InstructorName { get; set; }

        public bool IsOnline { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Average Rating")]
        public double AverageRating { get; set; } = 0;

        [Display(Name = "Rating Count")]
        public int RatingCount { get; set; } = 0;

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
