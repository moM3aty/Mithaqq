using System;
using System.ComponentModel.DataAnnotations;

namespace Mithaqq.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int? CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        [Range(1, 5)]
        public int Stars { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime DatePosted { get; set; } = DateTime.UtcNow;
    }
}
