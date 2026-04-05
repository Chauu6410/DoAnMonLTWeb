using DoAnMonLTWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnMonLTWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(product => product.Category)
                .OrderByDescending(product => product.Id)
                .ToListAsync();

            return View(products);
        }

        public IActionResult Create()
        {
            PopulateCategories();
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Image,Description,IsSale,Stock,CategoryId")] Product product, IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("Image", "Vui long chon hinh anh san pham.");
            }

            if (!ModelState.IsValid)
            {
                PopulateCategories(product.CategoryId);
                return View(product);
            }

            product.Image = await SaveImageAsync(imageFile!);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Them san pham thanh cong.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            PopulateCategories(product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Image,Description,IsSale,Stock,CategoryId")] Product product, IFormFile? imageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                PopulateCategories(product.CategoryId);
                return View(product);
            }

            var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                DeleteImage(existingProduct.Image);
                product.Image = await SaveImageAsync(imageFile);
            }
            else
            {
                product.Image = existingProduct.Image;
            }

            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cap nhat san pham thanh cong.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Products.AnyAsync(item => item.Id == id))
                {
                    return NotFound();
                }

                throw;
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(item => item.Category)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            DeleteImage(product.Image);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xoa san pham thanh cong.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(imageFile.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);
            return fileName;
        }

        private void DeleteImage(string? imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
            {
                return;
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products", imageName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        private void PopulateCategories(object? selectedCategory = null)
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(category => category.Name), "Id", "Name", selectedCategory);
        }
    }
}
