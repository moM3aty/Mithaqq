using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mithaqq.Models
{
    public class BlogPost
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [StringLength(100)]
        public string AuthorName { get; set; }

        public string ImageUrl { get; set; }

        [NotMapped]
        [Display(Name = "Main Image")]
        public IFormFile ImageFile { get; set; }

        public DateTime PublishDate { get; set; } = DateTime.UtcNow;

        [StringLength(300)]
        public string Subtitle { get; set; }

        public string AuthorTitle { get; set; }

        public string AuthorImageUrl { get; set; }

        [NotMapped]
        [Display(Name = "Author Image")]
        public IFormFile AuthorImageFile { get; set; }
    }
}

