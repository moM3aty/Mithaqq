using System.ComponentModel.DataAnnotations;

namespace Mithaqq.ViewModels
{
    public class ReviewViewModel
    {
        public int? ProductId { get; set; }
        public int? CourseId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Stars { get; set; }

        [StringLength(1000)]
        public string Comment { get; set; }
    }
}
