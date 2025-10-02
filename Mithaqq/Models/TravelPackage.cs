using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Mithaqq.Models
{
    public class TravelPackage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Destination { get; set; }

        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public int DurationDays { get; set; }

        public string Inclusions { get; set; } // Comma-separated values like "Flights, Hotels, Tours"

        public string ImageUrl { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }

        // Hardcode CompanyId to 4 for "EasyWay"
        public int CompanyId { get; set; } = 4;
        public virtual Company Company { get; set; }
    }
}
