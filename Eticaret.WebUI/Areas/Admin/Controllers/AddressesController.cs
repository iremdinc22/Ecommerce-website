using Eticaret.Core.Entities;
using Eticaret.Data;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AddressesController : Controller
    {
        private readonly IService<Address> _serviceAddress;
        private readonly DatabaseContext _context;

        public AddressesController(IService<Address> serviceAddress, DatabaseContext context)
        {
            _serviceAddress = serviceAddress;
            _context = context;
        }

        public IActionResult Index()
        {
            var addresses = _serviceAddress.GetQueryable()
                            .Include(a => a.AppUser)
                            .ToList();
            return View(addresses);
        }


        public IActionResult Create()
        {
            ViewBag.AppUserId = new SelectList(_context.AppUsers, "Id", "Email");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Address address)
        {
            if (ModelState.IsValid)
            {
                _serviceAddress.Add(address);
                return RedirectToAction(nameof(Index));
            }
            return View(address);
        }

        public IActionResult Edit(int id)
        {
            var address = _serviceAddress.Find(id);
            if (address == null)
            {
                return NotFound();
            }

            ViewBag.AppUserId = new SelectList(_context.AppUsers, "Id", "Email", address.AppUserId);
            return View(address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Address address)
        {
            if (ModelState.IsValid)
            {
                _serviceAddress.Update(address);
                return RedirectToAction(nameof(Index));
            }
            return View(address);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var address = _context.Addresses
                          .Include(a => a.AppUser) // 📌 AppUser'ı da getiriyoruz
                          .FirstOrDefault(a => a.Id == id);

            if (address == null)
                return NotFound();

            return View(address);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var address = _serviceAddress.Find(id);
            if (address != null)
            {
                _serviceAddress.Delete(address);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var address = _context.Addresses
                          .Include(a => a.AppUser) // 📌 Buraya dikkat!
                          .FirstOrDefault(a => a.Id == id);

            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

    }
}