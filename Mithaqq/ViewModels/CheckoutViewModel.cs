using Mithaqq.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json.Serialization;

namespace Mithaqq.ViewModels
{
    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Shipping Zone")]
        public int ShippingZoneId { get; set; }

        public IEnumerable<ShippingZone> ShippingZones { get; set; }

        public string PayPalClientId { get; set; }

        public string PaymentMethod { get; set; }
    }

    public class CourseCheckoutViewModel
    {
        public Course Course { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string ShippingAddress { get; set; }

        public string PayPalClientId { get; set; }
    }

    public class PaymentRequest
    {
        public int? CourseId { get; set; }
        public int? ShippingZoneId { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class CaptureOrderRequest
    {
        public string OrderId { get; set; }
        public int? CourseId { get; set; }
        public int? ShippingZoneId { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class PaypalOrderResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class PaypalCreateOrderResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}

