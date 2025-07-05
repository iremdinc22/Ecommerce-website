using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eticaret.Data;

public class NewsController : Controller
{
    private readonly DatabaseContext _context;

    public NewsController(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var newsList = await _context.News.ToListAsync();
        return View(newsList);
    }


    // GET: Admin/News/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound("Geçersiz İstek!");

        var news = await _context.News.FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
        if (news == null) return NotFound("Geçerli Bir Kamoanya Bulunamad!");

        return View(news);
    }


}