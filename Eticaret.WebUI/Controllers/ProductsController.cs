using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eticaret.Data;
using Eticaret.WebUI.Models;



public class ProductsController : Controller
{
    private readonly DatabaseContext _context;

    public ProductsController(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string q = "")
    {
        var model = new ProductListViewModel
        {
            Products = await _context.Products
                .Where(p => p.IsActive && (p.Name.Contains(q) || p.Description.Contains(q)))
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .ToListAsync()
        };
        return View(model); // ✅ sadece bu View döndürülmeli
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        var model = new ProductDetailViewModel()
        {
            Product = product,
            RelatedProducts = _context.Products
                .Where(p => p.IsActive && p.CategoryId == product.CategoryId && p.Id != product.Id)
                .ToList()
        };

        return View(model);
    }
}
