using Eticaret.Core.Entities;
using Eticaret.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Eticaret.WebUI.Utils;
using Microsoft.AspNetCore.Authorization;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin"),Authorize(Policy ="AdminPolicy")]
    public class NewsController : Controller
    {
        private readonly DatabaseContext _context;

        public NewsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/News
        public async Task<IActionResult> Index()
        {
            var newsList = await _context.News.ToListAsync();
            return View(newsList);
        }

        // GET: Admin/News/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News.FirstOrDefaultAsync(m => m.Id == id);
            if (news == null) return NotFound();

            return View(news);
        }

        // GET: Admin/News/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(News news, IFormFile? Image)
        {
            if (ModelState.IsValid)
            {
                if (Image is not null)
                {
                    news.Image = await FileHelper.FileLoaderAsync(Image, "/Img/News/");
                }
                else
                {
                    news.Image = "default.jpg"; // varsayılan görsel eğer boş geçildiyse
                }

                news.CreateDate = DateTime.Now;

                await _context.News.AddAsync(news);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // ModelState hatalarını logla
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Field: {state.Key} - Error: {error.ErrorMessage}");
                    }
                }
            }

            return View(news);
        }


        // GET: Admin/News/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News.FindAsync(id);
            if (news == null) return NotFound();

            return View(news);
        }

        // POST: Admin/News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, News news, IFormFile? Image, bool cbResmiSil = false)
        {
            if (id != news.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (cbResmiSil)
                    {
                        news.Image = string.Empty;
                    }
                    else if (Image is not null)
                    {
                        news.Image = await FileHelper.FileLoaderAsync(Image, "/Img/News/");
                    }
                    // Aksi halde mevcut image korunur, ekstra bir şey yapma


                    _context.Update(news);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.News.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(news);
        }

        // GET: Admin/News/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News.FirstOrDefaultAsync(m => m.Id == id);
            if (news == null) return NotFound();

            return View(news);
        }

        // POST: Admin/News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
