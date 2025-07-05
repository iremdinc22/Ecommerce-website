using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Eticaret.WebUI.Models;
using Eticaret.Data;
using Microsoft.EntityFrameworkCore;
using Eticaret.Core.Entities;
using Eticaret.WebUI.Utils;
using System.Threading.Tasks;

namespace Eticaret.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseContext _context;

        public HomeController(DatabaseContext context)
        {
            _context = context;
        }

        // Anasayfa
        public async Task<IActionResult> Index()
        {
            var model = new HomePageViewModel()
            {
                Sliders = await _context.Sliders.ToListAsync(),

                News = await _context.News
               .Where(news => news.IsActive)
               .ToListAsync(),

                Products = await _context.Products
               .Where(p => p.IsActive && p.IsHome)
               .ToListAsync()
              };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("AccessDenied")]

        public IActionResult AccessDenied()
        {
            return View();
        }

        // İletişim sayfası GET
        public IActionResult ContactUs()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ContactUs(Contact contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Contacts.AddAsync(contact);
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        TempData["Message"] = @"<div class='alert alert-success alert-dismissible fade show' role='alert'>
                    <strong>Mesajınız gönderildi!</strong> 
                    <button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>
                </div>";

                        // await MailHelper.SendMailAsync(contact);
                        return RedirectToAction("ContactUs", "Home");
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Bir hata oluştu. Lütfen tekrar deneyiniz.");
                }
            }

            return View(contact);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });


        }
    }
}


//http://localhost:5109/Home/ContactUs URL bu
