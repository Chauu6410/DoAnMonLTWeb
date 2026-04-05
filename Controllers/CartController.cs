using DoAnMonLTWeb.Helpers;
using DoAnMonLTWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace DoAnMonLTWeb.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ
        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            if (product.Stock <= 0)
            {
                TempData["ErrorMessage"] = $"San pham '{product.Name}' da het hang.";
                var referer = Request.Headers["Referer"].ToString();
                return !string.IsNullOrWhiteSpace(referer) ? Redirect(referer) : RedirectToAction("Index");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var item = cart.FirstOrDefault(c => c.Product.Id == id);
            if (item != null)
            {
                if (item.Quantity >= product.Stock)
                {
                    TempData["ErrorMessage"] = $"San pham '{product.Name}' chi con {product.Stock} sp trong kho.";
                    var referer = Request.Headers["Referer"].ToString();
                    return !string.IsNullOrWhiteSpace(referer) ? Redirect(referer) : RedirectToAction("Index");
                }

                item.Quantity++;
            }
            else
            {
                cart.Add(new CartItem { Product = product, Quantity = 1 });
            }

            // Cập nhật lại Session sau khi thay đổi giỏ hàng
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ
        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            cart.RemoveAll(c => c.Product.Id == id);

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            return RedirectToAction("Index");
        }

        // Cập nhật số lượng sản phẩm trong giỏ
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.Product.Id == id);
            if (item != null)
            {
                if (quantity > 0)
                {
                    var product = _context.Products.FirstOrDefault(p => p.Id == id);
                    if (product == null)
                    {
                        return NotFound();
                    }

                    if (quantity > product.Stock)
                    {
                        TempData["ErrorMessage"] = $"So luong yeu cau vuot qua ton kho. San pham '{product.Name}' chi con {product.Stock} sp.";
                        item.Quantity = product.Stock > 0 ? product.Stock : 1;
                    }
                    else
                    {
                        item.Quantity = quantity;
                    }
                }
                else
                {
                    cart.Remove(item);
                }
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            return RedirectToAction("Index");
        }

    }
    
}
