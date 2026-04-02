using DoAnMonLTWeb.Helpers;
using DoAnMonLTWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            // Gán sẵn thông tin khách hàng nếu đã đăng nhập
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
            // Sửa lại: Nếu lỗi session (giỏ hàng trống), redirect thẳng ra trang giỏ hàng
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Cart = cart;
                return View("Index", order);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                order.UserId = currentUser.Id; // Liên kết đơn hàng với tài khoản (Nên có UserId trong Model Order)
            }

            order.OrderDate = DateTime.Now;
            order.Status = OrderStatusHelper.ChoXuLy;
            order.Notes ??= string.Empty;
            
            // Tính tổng giá trị đơn hàng
            order.TotalAmount = cart.Sum(item => item.Product.Price * item.Quantity);

            // Gán list chi tiết đơn hàng trực tiếp để EF Core tự tạo Transaction 1 lần SaveChanges
            order.OrderDetails = cart.Select(item => new OrderDetail
            {
                ProductId = item.Product.Id,
                Quantity = item.Quantity,
                Price = item.Product.Price 
            }).ToList();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Lưu 1 lần duy nhất

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
