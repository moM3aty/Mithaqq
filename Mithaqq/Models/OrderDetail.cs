using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mithaqq.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int? CourseId { get; set; }
        public virtual Course Course { get; set; }

        public int? TravelPackageId { get; set; } // Added this
        public virtual TravelPackage TravelPackage { get; set; } // Added this

        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
}
