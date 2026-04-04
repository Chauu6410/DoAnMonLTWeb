using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAnMonLTWeb.Models;

namespace DoAnMonLTWeb.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [Authorize(Roles = "Admin")]
        // GET: Products/Create
        public IActionResult Create()
        {
            return RedirectToAction("Create", "Products", new { area = "Admin" });
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Name,Price,Image,Description,CategoryId")] Product product)
        {
            return RedirectToAction("Create", "Products", new { area = "Admin" });
        }

        [Authorize(Roles = "Admin")]
        // GET: Products/Edit/5
        public IActionResult Edit(int? id)
        {
            return RedirectToAction("Edit", "Products", new { area = "Admin", id });
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Name,Price,Image,Description,CategoryId")] Product product)
        {
            return RedirectToAction("Edit", "Products", new { area = "Admin", id });
        }

        [Authorize(Roles = "Admin")]
        // GET: Products/Delete/5
        public IActionResult Delete(int? id)
        {
            return RedirectToAction("Delete", "Products", new { area = "Admin", id });
        }

        // POST: Products/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            return RedirectToAction("Delete", "Products", new { area = "Admin", id });
        }
        public IActionResult Search(string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return View(new List<Product>());
            }

            var normalizedKeyword = keyword.Trim();

            var products = _context.Products
                .Where(p => p.Name.Contains(normalizedKeyword))
                .ToList();

            return View(products);
        }
        public IActionResult Sale()
        {
            var products = _context.Products
                .Where(p => p.IsSale == true)
                .ToList();

            return View("Index", products);
        }
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
