using System;
using System.Collections.Generic;

namespace Mithaqq.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime DateCreated { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
