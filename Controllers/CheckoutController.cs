using DoAnMonLTWeb.Helpers;
using DoAnMonLTWeb.Models;
using Microsoft.AspNetCore.Mvc;
namespace DoAnMonLTWeb.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            ViewBag.Cart = cart;
            return View();
        }

        [HttpPost]
        public IActionResult PlaceOrder(Order order)
        {

            // Xóa giỏ hàng
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
