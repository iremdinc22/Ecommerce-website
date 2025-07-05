using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Eticaret.WebUI.Controllers
{
    [Authorize]
    public class MyAddressesController : Controller
    {
        private readonly IService<AppUser> _serviceAppUser;
        private readonly IService<Address> _serviceAddress;

        public MyAddressesController(IService<AppUser> service, IService<Address> Addressservice)
        {
            _serviceAppUser = service;
            _serviceAddress = Addressservice;
        }

        public async Task<IActionResult> Index()
        {
            var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
            if (appUser == null)
            {
                return NotFound("Kullanıcı Datası Bulunamadı ! Oturumunuzu Kapatıp Lütfen Tekrar Giriş Yapın!");
            }
            var model = await _serviceAddress.GetAllAsync(u => u.AppUserId == appUser.Id);
            return View(model);
        }

        // GET: Address/Create
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Address address)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() ==
                        HttpContext.User.FindFirst("UserGuid").Value);
                    if (appUser != null)
                    {
                        address.AppUserId = appUser.Id;
                        _serviceAddress.Add(address);
                        await _serviceAddress.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Hata Oluştu!");
                }
            }


            ModelState.AddModelError("", "Kayıt Başarısız!");
            return View(address);
        }


        public async Task<IActionResult> Edit(string id)
        {
            var claim = HttpContext.User.FindFirst("UserGuid");
            if (claim == null)
                return RedirectToAction("Login", "Account");

            var appUser = await _serviceAppUser.GetAsync(
                x => x.UserGuid.ToString() == claim.Value);

            if (appUser == null)
            {
                return NotFound("Kullanıcı Datası Bulunamadı! Oturumunuzu Kapatıp Lütfen Tekrar Giriş Yapın!");
            }

            if (!Guid.TryParse(id, out Guid parsedGuid))
                return BadRequest("Geçersiz adres ID");

            var model = await _serviceAddress.GetAsync(
                u => u.AddressGuid == parsedGuid && u.AppUserId == appUser.Id);

            if (model == null)
                return NotFound("Adres bilgisi bulunamadı."); // ✅ Kritik satır

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Address address)
        {
            var claim = HttpContext.User.FindFirst("UserGuid");
            if (claim == null)
                return RedirectToAction("Login", "Account");

            if (!Guid.TryParse(claim.Value, out Guid userGuid))
                return Unauthorized("Geçersiz oturum bilgisi.");

            Console.WriteLine($"[DEBUG] Claim'den gelen UserGuid: {claim?.Value}");

            var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == claim.Value);
            if (appUser == null)
                return NotFound("Kullanıcı Datası Bulunamadı! Oturumu kapatıp tekrar giriş yapın.");

            if (!Guid.TryParse(id, out Guid parsedGuid))
                return BadRequest("Geçersiz adres ID");

            var model = await _serviceAddress.GetAsync(u => u.AddressGuid == parsedGuid && u.AppUserId == appUser.Id);
            if (model == null)
                return NotFound("Adres Bilgisi Bulunamadı!");

            // Güncelleme
            model.Title = address.Title;
            model.District = address.District;
            model.City = address.City;
            model.OpenAddress = address.OpenAddress;
            model.IsDeliveryAddress = address.IsDeliveryAddress;
            model.IsBillingAddress = address.IsBillingAddress;
            model.IsActive = address.IsActive;

            // Diğer adreslerin fatura/teslimat bayraklarını sıfırla
            var otherAddresses = await _serviceAddress.GetAllAsync(x => x.AppUserId == appUser.Id && x.Id != model.Id);
            foreach (var otherAddress in otherAddresses)
            {
                otherAddress.IsDeliveryAddress = false;
                otherAddress.IsBillingAddress = false;
                _serviceAddress.Update(otherAddress);
            }

            try
            {
                _serviceAddress.Update(model);
                await _serviceAddress.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Güncelleme sırasında hata oluştu!");
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            var claim = HttpContext.User.FindFirst("UserGuid");
            if (claim == null)
                return RedirectToAction("Login", "Account");

            var appUser = await _serviceAppUser.GetAsync(
                x => x.UserGuid.ToString() == claim.Value);

            if (appUser == null)
            {
                return NotFound("Kullanıcı Datası Bulunamadı! Oturumunuzu Kapatıp Lütfen Tekrar Giriş Yapın!");
            }

            if (!Guid.TryParse(id, out Guid parsedGuid))
                return BadRequest("Geçersiz adres ID");

            var model = await _serviceAddress.GetAsync(
                u => u.AddressGuid == parsedGuid && u.AppUserId == appUser.Id);

            if (model == null)
                return NotFound("Adres bilgisi bulunamadı.");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Delete(string id, Address address)
        {
            var claim = HttpContext.User.FindFirst("UserGuid");
            if (claim == null)
                return RedirectToAction("Login", "Account");

            var appUser = await _serviceAppUser.GetAsync(
                x => x.UserGuid.ToString() == claim.Value);

            if (appUser == null)
            {
                return NotFound("Kullanıcı Datası Bulunamadı! Oturumunuzu Kapatıp Lütfen Tekrar Giriş Yapın!");
            }

            if (!Guid.TryParse(id, out Guid parsedGuid))
                return BadRequest("Geçersiz adres ID");

            var model = await _serviceAddress.GetAsync(
                u => u.AddressGuid == parsedGuid && u.AppUserId == appUser.Id);

            if (model == null)
                return NotFound("Adres bilgisi bulunamadı.");
            try
            {
                _serviceAddress.Delete(model);
                await _serviceAddress.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            catch (Exception)
            {
                ModelState.AddModelError("", "Silme sırasında hata oluştu!");
            }

            return View(model);
        }

    }


}


