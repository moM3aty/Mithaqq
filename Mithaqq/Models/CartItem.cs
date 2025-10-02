using System.ComponentModel.DataAnnotations.Schema;

namespace Mithaqq.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        public virtual Cart Cart { get; set; }

        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int? CourseId { get; set; }
        public virtual Course Course { get; set; }

        public int? TravelPackageId { get; set; }
        public virtual TravelPackage TravelPackage { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Price at the time of adding to cart
    }
}
