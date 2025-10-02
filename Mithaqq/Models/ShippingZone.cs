using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mithaqq.Models
{
    public class ShippingZone
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ZoneName { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; }
    }
}
