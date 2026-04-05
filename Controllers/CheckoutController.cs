using DoAnMonLTWeb.Helpers;
using DoAnMonLTWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnMonLTWeb.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var order = new Order();
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                order.CustomerName = currentUser.FullName;
                order.Phone = currentUser.PhoneNumber ?? string.Empty;
                order.ShippingAddress = currentUser.Address ?? string.Empty;
            }

            ViewBag.Cart = cart;
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Cart = cart;
                return View("Index", order);
            }

            foreach (var cartItem in cart)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == cartItem.Product.Id);
                if (product == null)
                {
                    ModelState.AddModelError(string.Empty, $"Khong tim thay san pham co ID {cartItem.Product.Id}.");
                    ViewBag.Cart = cart;
                    return View("Index", order);
                }

                if (product.Stock < cartItem.Quantity)
                {
                    ModelState.AddModelError(string.Empty, $"San pham '{product.Name}' chi con {product.Stock} sp trong kho.");
                    ViewBag.Cart = cart;
                    return View("Index", order);
                }
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                order.UserId = currentUser.Id;
            }

            order.OrderDate = DateTime.Now;
            order.Status = OrderStatusHelper.ChoXuLy;
            order.Notes ??= string.Empty;
            order.TotalAmount = cart.Sum(item => item.Product.Price * item.Quantity);

            order.OrderDetails = cart.Select(item => new OrderDetail
            {
                ProductId = item.Product.Id,
                Quantity = item.Quantity,
                Price = item.Product.Price
            }).ToList();

            foreach (var cartItem in cart)
            {
                var product = await _context.Products.FirstAsync(p => p.Id == cartItem.Product.Id);
                product.Stock -= cartItem.Quantity;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["LastOrderId"] = order.Id;
            HttpContext.Session.Remove("Cart");
            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
