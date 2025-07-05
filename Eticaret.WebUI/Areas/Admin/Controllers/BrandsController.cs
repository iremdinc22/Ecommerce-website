using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eticaret.Data;
using Eticaret.Core.Entities;
using Eticaret.WebUI.Utils;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin"),Authorize(Policy ="AdminPolicy")]
    public class BrandsController : Controller
    {
        private readonly DatabaseContext _context;

        public BrandsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/Brands
        public async Task<IActionResult> Index()
        {
            return View(await _context.Brands.ToListAsync());
        }

        // GET: Admin/Brands/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var brand = await _context.Brands.FirstOrDefaultAsync(m => m.Id == id);
            if (brand == null) return NotFound();

            return View(brand);

        }

        // GET: Admin/Brands/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Brands/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create(Brand brand, IFormFile? Logo)
        //{
        //if (ModelState.IsValid)
        // {
        // brand.Logo = await FileHelper.FileLoaderAsync(Logo, "/Img/Brands/");
        // _context.Add(brand);
        //  await _context.SaveChangesAsync();
        //  return RedirectToAction(nameof(Index));
        // }
        //return View(brand);
        //}

        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Brand brand, IFormFile? Logo)
{
    if (ModelState.IsValid)
    {
        if (Logo != null && Logo.Length > 0)
        {
            var path = await FileHelper.FileLoaderAsync(Logo, "/Img/Brands/");
            brand.Logo = path;
        }

        brand.CreateDate = DateTime.Now;
        _context.Add(brand);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ❗ HATALAR BURADA YAZILACAK
    foreach (var kvp in ModelState)
    {
        foreach (var error in kvp.Value.Errors)
        {
            Console.WriteLine($"⛔ HATA → Alan: {kvp.Key} → {error.ErrorMessage}");
        }
    }

    return View(brand);
}





        // GET: Admin/Brands/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return NotFound();

            return View(brand);
        }

        // POST: Admin/Brands/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand brand, IFormFile? Logo, bool cbResmiSil = false)
        {
            if (id != brand.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (cbResmiSil)
                        brand.Logo = string.Empty;
                    if (Logo is not null)
                        brand.Logo = await FileHelper.FileLoaderAsync(Logo, "/Img/Brands/");
                    _context.Update(brand);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Brands.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(brand);
        }


        // GET: Admin/Brands/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var brand = await _context.Brands
                .FirstOrDefaultAsync(m => m.Id == id);

            if (brand == null)
                return NotFound();

            return View(brand);
        }




        // POST: Admin/Brands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {
                if (!string.IsNullOrEmpty(brand.Logo))
                {
                    FileHelper.FileRemover(brand.Logo);
                }

                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
