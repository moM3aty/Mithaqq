using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mithaqq.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderTotal { get; set; }

        [Required]
        public string Status { get; set; } // e.g., Pending, Processing, Shipped, Completed, CashOnDelivery

        [Required]
        public string ShippingAddress { get; set; }

        public string PhoneNumber { get; set; }

        public string PaymentMethod { get; set; } // e.g., PayPal, Stripe, CashOnDelivery

        public string? PaymentId { get; set; } // For PayPal or Stripe Transaction ID

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
