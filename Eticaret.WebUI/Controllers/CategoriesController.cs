using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eticaret.Data;

public class CategoriesController : Controller
{
    private readonly DatabaseContext _context;

    public CategoriesController(DatabaseContext context)
    {
        _context = context;
    }

     public async Task<IActionResult> IndexAsync(int? id)
    {
        if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categorys.Include(P=>P.Products)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
    }

 
}