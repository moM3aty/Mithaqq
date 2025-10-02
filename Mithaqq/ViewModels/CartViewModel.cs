using Mithaqq.Models;
using System.Collections.Generic;
using System.Linq;

namespace Mithaqq.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal Subtotal => CartItems.Sum(item => item.Price * item.Quantity);
        public decimal DeliveryCost { get; set; } = 25.00m; 
        public decimal Tax { get; set; } = 14.00m; 
        public decimal Discount { get; set; } = 60.00m;
        public decimal Total => Subtotal + DeliveryCost + Tax - Discount;
    }
}
