using DoAnMonLTWeb.Helpers;
using DoAnMonLTWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnMonLTWeb.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string CartSessionKey = "Cart";

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey);
            if (cart == null || !cart.Any())
            {
                ModelState.AddModelError("", "Gi? hàng tr?ng, không th? d?t hàng.");
                return View(order);
            }

            order.OrderDate = DateTime.Now;
            order.Status = OrderStatusHelper.ChoXuLy;
            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in cart)
            {
                var detail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                };
                _context.OrderDetails.Add(detail);
            }
            _context.SaveChanges();

            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var fullName = user.FullName?.Trim();
            if (string.IsNullOrEmpty(fullName))
            {
                return View(new List<Order>());
            }

            var orders = await _context.Orders
                .Include(order => order.OrderDetails)
                .Where(order => order.CustomerName == fullName)
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [Authorize]
        public async Task<IActionResult> MyOrderDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var fullName = user.FullName?.Trim();
            if (string.IsNullOrEmpty(fullName))
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(item => item.OrderDetails)
                .ThenInclude(detail => detail.Product)
                .FirstOrDefaultAsync(item => item.Id == id && item.CustomerName == fullName);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var fullName = user.FullName?.Trim();
            if (string.IsNullOrEmpty(fullName))
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerName == fullName);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == OrderStatusHelper.ChoXuLy)
            {
                order.Status = OrderStatusHelper.DaHuy;
                _context.Update(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hủy đơn hàng thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng này vì đang được xử lý hoặc đã hoàn thành.";
            }

            return RedirectToAction(nameof(MyOrders));
        }
    }
}
