using DoAnMonLTWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnMonLTWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ProductCount = await _context.Products.CountAsync();
            ViewBag.CategoryCount = await _context.Categories.CountAsync();
            ViewBag.OrderCount = await _context.Orders.CountAsync();
            ViewBag.PendingOrderCount = await _context.Orders.CountAsync(order => order.Status == OrderStatusHelper.ChoXuLy || order.Status == "Pending");

            var latestOrders = await _context.Orders
                .OrderByDescending(order => order.OrderDate)
                .Take(5)
                .ToListAsync();

            foreach (var order in latestOrders)
            {
                order.Status = OrderStatusHelper.Normalize(order.Status);
            }

            return View(latestOrders);
        }
    }
}
