using DoAnMonLTWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnMonLTWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(order => order.OrderDetails)
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();

            foreach (var order in orders)
            {
                order.Status = OrderStatusHelper.Normalize(order.Status);
            }

            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(item => item.OrderDetails)
                .ThenInclude(detail => detail.Product)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = OrderStatusHelper.Normalize(order.Status);
            ViewBag.AvailableStatuses = OrderStatusHelper.All;
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = OrderStatusHelper.Normalize(status);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cap nhat trang thai don hang thanh cong.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
