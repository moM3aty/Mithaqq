﻿using System.ComponentModel.DataAnnotations;

namespace Mithaqq.Models
{
    public class MarketerPackage
    {
        public int Id { get; set; }

        [Required]
        public string MarketerId { get; set; }
        public virtual ApplicationUser Marketer { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
