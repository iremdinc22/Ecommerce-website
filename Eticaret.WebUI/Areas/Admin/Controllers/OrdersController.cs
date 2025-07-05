using Eticaret.Core.Entities;
using Eticaret.Core.Enums;
using Eticaret.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // selectli kısım için gerekiyor
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class OrdersController : Controller
    {
        private readonly DatabaseContext _context;

        public OrdersController(DatabaseContext context)
        {
            _context = context;
        }

        // ✅ Enum için Türkçe dropdown listesi döndürür
        private List<SelectListItem> GetOrderStateSelectList()
        {
            return Enum.GetValues(typeof(EnumOrderState))
                .Cast<EnumOrderState>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.GetType()
                        .GetMember(e.ToString())[0]
                        .GetCustomAttribute<DisplayAttribute>()?.GetName() ?? e.ToString()
                }).ToList();
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index()
        {
            var orderList = await _context.Orders
                .Include(o => o.AppUser)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orderList);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.AppUser)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: Admin/Orders/Details/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, Order updatedOrder)
        {
            if (id != updatedOrder.Id) return NotFound();

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.DeliveryAddress = updatedOrder.DeliveryAddress;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            ViewBag.OrderStates = GetOrderStateSelectList();
            return View(order);
        }

        // POST: Admin/Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order updatedOrder)
        {
            if (id != updatedOrder.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.OrderStates = GetOrderStateSelectList();
                return View(updatedOrder);
            }

            try
            {
                _context.Update(updatedOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(e => e.Id == updatedOrder.Id))
                    return NotFound();

                ModelState.AddModelError("", "Hata oluştu.");
                ViewBag.OrderStates = GetOrderStateSelectList();
                return View(updatedOrder);
            }
        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order != null)
            {
                var lines = _context.OrderLines.Where(x => x.OrderId == order.Id);
                _context.OrderLines.RemoveRange(lines);

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Orders/FixAddressTexts
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FixAddressTexts()
        {
            var orders = _context.Orders.ToList();

            foreach (var order in orders)
            {
                if (Guid.TryParse(order.BillingAddress, out Guid billingAddressGuid))
                {
                    var address = _context.Addresses.FirstOrDefault(a => a.AddressGuid == billingAddressGuid);
                    if (address != null)
                    {
                        order.BillingAddress = $"{address.OpenAddress}, {address.District}, {address.City}";
                    }
                }

                if (Guid.TryParse(order.DeliveryAddress, out Guid deliveryAddressGuid))
                {
                    var address = _context.Addresses.FirstOrDefault(a => a.AddressGuid == deliveryAddressGuid);
                    if (address != null)
                    {
                        order.DeliveryAddress = $"{address.OpenAddress}, {address.District}, {address.City}";
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Adres bilgileri başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
