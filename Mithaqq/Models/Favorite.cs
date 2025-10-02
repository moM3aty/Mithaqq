using System.ComponentModel.DataAnnotations;

namespace Mithaqq.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int? CourseId { get; set; }
        public virtual Course Course { get; set; }
    }
}
