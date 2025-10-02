using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mithaqq.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Price Before Discount")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Price After Discount")]
        public decimal? SalePrice { get; set; }

        [Display(Name = "Discount Percentage")]
        public int? DiscountPercentage { get; set; }

        public string ImageUrl { get; set; }

        [NotMapped]
        [Display(Name = "Product Image")]
        public IFormFile ImageFile { get; set; }

        [Display(Name = "Available Stock")]
        public int StockQuantity { get; set; } = 0;

        public int CompanyId { get; set; }
        public int CategoryId { get; set; }

        public virtual Company Company { get; set; }
        public virtual Category Category { get; set; }

        [Display(Name = "Average Rating")]
        public double AverageRating { get; set; } = 0;

        [Display(Name = "Rating Count")]
        public int RatingCount { get; set; } = 0;

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}

