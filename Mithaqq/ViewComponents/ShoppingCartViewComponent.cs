using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mithaqq.Data;
using Mithaqq.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Mithaqq.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsPrincipal = (ClaimsPrincipal)User;
            var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItemCount = 0;

            if (userId != null)
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart != null)
                {
                    cartItemCount = cart.CartItems.Sum(ci => ci.Quantity);
                }
            }

            return View(cartItemCount);
        }
    }
}
