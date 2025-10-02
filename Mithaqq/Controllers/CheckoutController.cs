using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mithaqq.Data;
using Mithaqq.Models;
using Mithaqq.Services;
using Mithaqq.ViewModels;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Mithaqq.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PaypalService _paypalService;
        private readonly IConfiguration _configuration;

        public CheckoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, PaypalService paypalService, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _paypalService = paypalService;
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["NavbarSolid"] = true;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var cart = await _context.Carts
                .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                .Include(c => c.CartItems).ThenInclude(ci => ci.Course)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Index", "Store");
            }

            var viewModel = new CheckoutViewModel
            {
                Cart = new CartViewModel { CartItems = cart.CartItems.ToList() },
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ShippingAddress = user.Address ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                ShippingZones = await _context.ShippingZones.ToListAsync(),
                PayPalClientId = _paypalService.GetClientId()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }
            return RedirectToAction("CourseCheckout", new { courseId });
        }

        [HttpGet]
        public async Task<IActionResult> CourseCheckout(int courseId)
        {
            ViewData["NavbarSolid"] = true;
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            var viewModel = new CourseCheckoutViewModel
            {
                Course = course,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ShippingAddress = user.Address ?? "",
                PayPalClientId = _paypalService.GetClientId()
            };
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (model.PaymentMethod == "CashOnDelivery")
            {
                var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
                if (cart == null || !cart.CartItems.Any()) return BadRequest("Your cart is empty.");

                await CreateOrderFromCart(user, cart, model, "CashOnDelivery", "Processing", null);
                return RedirectToAction("Success");
            }

            return RedirectToAction("Index");
        }

        [HttpGet("api/checkout/shippingcost/{zoneId}")]
        public async Task<IActionResult> GetShippingCost(int zoneId)
        {
            var zone = await _context.ShippingZones.FindAsync(zoneId);
            if (zone == null)
            {
                return NotFound();
            }
            return Ok(new { cost = zone.ShippingCost });
        }


        [HttpPost("api/checkout/create-paypal-order")]
        public async Task<IActionResult> CreatePayPalOrder([FromBody] PaymentRequest request)
        {
            try
            {
                decimal total;
                decimal shippingCost = 0;

                if (request.CourseId.HasValue)
                {
                    var course = await _context.Courses.FindAsync(request.CourseId.Value);
                    if (course == null) return BadRequest(new { error = new { message = "Course not found." } });
                    total = course.SalePrice ?? course.Price;
                }
                else
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var cart = await _context.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Product).Include(c => c.CartItems).ThenInclude(ci => ci.Course).FirstOrDefaultAsync(c => c.UserId == userId);
                    if (cart == null || !cart.CartItems.Any()) return BadRequest(new { error = new { message = "Cart is empty." } });
                    total = new CartViewModel { CartItems = cart.CartItems.ToList() }.Subtotal;

                    if (request.ShippingZoneId.HasValue)
                    {
                        var zone = await _context.ShippingZones.FindAsync(request.ShippingZoneId.Value);
                        if (zone != null) shippingCost = zone.ShippingCost;
                    }
                    total += shippingCost;
                }

                var response = await _paypalService.CreateOrderAsync(total, "USD");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = new { message = ex.Message } });
            }
        }

        [HttpPost("api/checkout/capture-paypal-order")]
        public async Task<IActionResult> CapturePayPalOrder([FromBody] CaptureOrderRequest request)
        {
            try
            {
                var response = await _paypalService.CaptureOrderAsync(request.OrderId);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (response.Status == "COMPLETED")
                {
                    if (request.CourseId.HasValue)
                    {
                        var course = await _context.Courses.FindAsync(request.CourseId.Value);
                        if (course == null) return BadRequest(new { message = "Course not found." });
                        await CreateOrderForCourse(user, course, "PayPal", "Pending Approval", response.Id);
                    }
                    else
                    {
                        var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
                        if (cart == null) return BadRequest(new { message = "Cart not found." });

                        var checkoutModel = new CheckoutViewModel
                        {
                            ShippingZoneId = request.ShippingZoneId ?? 0,
                            ShippingAddress = request.ShippingAddress,
                            PhoneNumber = request.PhoneNumber
                        };
                        await CreateOrderFromCart(user, cart, checkoutModel, "PayPal", "Processing", response.Id);
                    }
                    return Ok(new { success = true });
                }

                return BadRequest(new { message = "PayPal payment could not be completed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("api/checkout/create-stripe-session")]
        public async Task<IActionResult> CreateStripeCheckoutSession([FromBody] PaymentRequest request)
        {
            try
            {
                var domain = $"{Request.Scheme}://{Request.Host}";
                var lineItems = new List<SessionLineItemOptions>();
                var metadata = new Dictionary<string, string>();

                if (request.CourseId.HasValue)
                {
                    var course = await _context.Courses.FindAsync(request.CourseId.Value);
                    if (course == null) return BadRequest(new { error = new { message = "Course not found." } });
                    lineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)((course.SalePrice ?? course.Price) * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions { Name = course.Name },
                        },
                        Quantity = 1,
                    });
                    metadata.Add("CourseId", course.Id.ToString());
                }
                else
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var cart = await _context.Carts
                        .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                        .Include(c => c.CartItems).ThenInclude(ci => ci.Course)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (cart == null || !cart.CartItems.Any()) return BadRequest(new { error = new { message = "Cart is empty." } });

                    lineItems.AddRange(cart.CartItems.Select(item => new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions { Name = item.Product?.Name ?? item.Course?.Name },
                        },
                        Quantity = item.Quantity,
                    }));

                    if (request.ShippingZoneId.HasValue)
                    {
                        var zone = await _context.ShippingZones.FindAsync(request.ShippingZoneId.Value);
                        if (zone != null)
                        {
                            lineItems.Add(new SessionLineItemOptions
                            {
                                PriceData = new SessionLineItemPriceDataOptions
                                {
                                    UnitAmount = (long)(zone.ShippingCost * 100),
                                    Currency = "usd",
                                    ProductData = new SessionLineItemPriceDataProductDataOptions { Name = "Shipping" },
                                },
                                Quantity = 1,
                            });
                        }
                    }
                    metadata.Add("ShippingZoneId", request.ShippingZoneId?.ToString() ?? "0");
                    metadata.Add("ShippingAddress", request.ShippingAddress ?? "");
                    metadata.Add("PhoneNumber", request.PhoneNumber ?? "");
                }

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = lineItems,
                    Mode = "payment",
                    SuccessUrl = domain + "/Checkout/StripeSuccess?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = domain + (request.CourseId.HasValue ? $"/Checkout/CourseCheckout?courseId={request.CourseId.Value}" : "/Checkout/Index"),
                    Metadata = metadata
                };

                var service = new SessionService();
                Session session = await service.CreateAsync(options);
                return Ok(new { sessionId = session.Id });
            }
            catch (StripeException e)
            {
                // This will catch the "Invalid API Key" error and return it cleanly.
                return BadRequest(new { error = new { message = e.Message } });
            }
            catch (Exception ex)
            {
                // Catch any other unexpected server errors.
                return StatusCode(500, new { error = new { message = "An internal server error occurred." } });
            }
        }


        [HttpGet]
        public async Task<IActionResult> StripeSuccess(string session_id)
        {
            var sessionService = new SessionService();
            Session session = await sessionService.GetAsync(session_id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (session.PaymentStatus == "paid")
            {
                var hasCourseId = session.Metadata.TryGetValue("CourseId", out var courseIdStr);
                if (hasCourseId && int.TryParse(courseIdStr, out var courseId))
                {
                    var course = await _context.Courses.FindAsync(courseId);
                    if (course == null) return BadRequest("Course not found.");
                    await CreateOrderForCourse(user, course, "Stripe", "Pending Approval", session.PaymentIntentId);
                }
                else
                {
                    var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
                    if (cart != null)
                    {
                        var checkoutModel = new CheckoutViewModel
                        {
                            ShippingZoneId = int.TryParse(session.Metadata["ShippingZoneId"], out var zoneId) ? zoneId : 0,
                            ShippingAddress = session.Metadata["ShippingAddress"],
                            PhoneNumber = session.Metadata["PhoneNumber"]
                        };
                        await CreateOrderFromCart(user, cart, checkoutModel, "Stripe", "Processing", session.PaymentIntentId);
                    }
                }
                return RedirectToAction("Success");
            }
            return RedirectToAction("Index");
        }

        private async Task CreateOrderFromCart(ApplicationUser user, Cart cart, CheckoutViewModel checkoutModel, string paymentMethod, string status, string? paymentId)
        {
            var cartViewModel = new CartViewModel { CartItems = cart.CartItems.ToList() };
            decimal shippingCost = 0;
            if (checkoutModel.ShippingZoneId > 0)
            {
                var zone = await _context.ShippingZones.FindAsync(checkoutModel.ShippingZoneId);
                if (zone != null)
                {
                    shippingCost = zone.ShippingCost;
                }
            }

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                OrderTotal = cartViewModel.Subtotal + shippingCost,
                Status = status,
                ShippingAddress = checkoutModel.ShippingAddress,
                PhoneNumber = checkoutModel.PhoneNumber,
                PaymentMethod = paymentMethod,
                PaymentId = paymentId,
                OrderDetails = cart.CartItems.Select(ci => new OrderDetail
                {
                    ProductId = ci.ProductId,
                    CourseId = ci.CourseId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();
        }

        private async Task CreateOrderForCourse(ApplicationUser user, Course course, string paymentMethod, string status, string? paymentId)
        {
            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                OrderTotal = course.SalePrice ?? course.Price,
                Status = status,
                ShippingAddress = "Digital Delivery",
                PhoneNumber = user.PhoneNumber,
                PaymentMethod = paymentMethod,
                PaymentId = paymentId,
                OrderDetails = new List<OrderDetail>
                {
                    new()
                    {
                        CourseId = course.Id,
                        Quantity = 1,
                        UnitPrice = course.SalePrice ?? course.Price
                    }
                }
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }


        [HttpGet]
        public IActionResult Success()
        {
            ViewData["NavbarSolid"] = true;
            return View();
        }
    }
}

