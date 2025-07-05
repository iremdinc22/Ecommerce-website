using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eticaret.Data;
using Eticaret.Core.Entities;
using Eticaret.WebUI.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication; //loginle alakalı
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Authentication.Cookies;
using Eticaret.Service.Abstract;
using Eticaret.WebUI.Utils; //Login methodunda kullandık

namespace Eticaret.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseContext _context;

        public AccountController(DatabaseContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            var userGuid = HttpContext.User.FindFirst("UserGuid")?.Value;
            Console.WriteLine("🔍 Cookie’den gelen UserGuid: " + userGuid);

            if (string.IsNullOrEmpty(userGuid))
            {
                return Content("❌ Cookie’de UserGuid yok.");
            }

            var user = _context.AppUsers
                .AsEnumerable()
                .FirstOrDefault(x => x.UserGuid.ToString().Equals(userGuid, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return Content($"❌ UserGuid bulundu ama veritabanında eşleşen kullanıcı yok.\nUserGuid: {userGuid}");
            }

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Phone = user.Phone,
                Password = user.Password
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> SignInAsync(LoginViewModel loginViewModel)
        {
            Console.WriteLine(">>> Giriş denemesi başladı");
            Console.WriteLine("Gelen Email: " + loginViewModel.Email);
            Console.WriteLine("Gelen Password: " + loginViewModel.Password);

            if (ModelState.IsValid)
            {
                try
                {
                    var account = await _context.AppUsers.FirstOrDefaultAsync(x =>
                        x.Email == loginViewModel.Email &&
                        x.Password == loginViewModel.Password &&
                        x.IsActive);

                    if (account == null)
                    {
                        Console.WriteLine("❌ Kullanıcı bulunamadı: E-posta veya şifre eşleşmedi.");
                        ModelState.AddModelError("", "Giriş Başarısız!");
                    }
                    else
                    {
                        Console.WriteLine("✅ Kullanıcı bulundu: " + account.Email);
                        Console.WriteLine("UserGuid: " + account.UserGuid);

                        var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, account.Name),
                    new(ClaimTypes.Role, account.IsAdmin ? "Admin" : "Customer"),
                    new(ClaimTypes.Email, account.Email),
                    new("UserId", account.Id.ToString()),
                    new("UserGuid", account.UserGuid.ToString())
                };

                        var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var userPrincipal = new ClaimsPrincipal(userIdentity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal);

                        return Redirect(string.IsNullOrEmpty(loginViewModel.ReturnUrl) ? "/" : loginViewModel.ReturnUrl);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("🔥 Hata: " + ex.Message);
                    ModelState.AddModelError("", "Hata Oluştu!");
                }
            }
            else
            {
                Console.WriteLine("❗ ModelState geçersiz.");
            }

            return View(loginViewModel);
        }


        [Authorize]
        public IActionResult MyOrders()
        {
            var userGuid = HttpContext.User.FindFirst("UserGuid")?.Value;
            Console.WriteLine("🔍 Cookie’den gelen UserGuid: " + userGuid);

            if (string.IsNullOrEmpty(userGuid))
            {
                return RedirectToAction("SignIn");
            }

            var user = _context.AppUsers
                .AsEnumerable()
                .FirstOrDefault(x => x.UserGuid.ToString().Equals(userGuid, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return Content($"❌ UserGuid bulundu ama veritabanında eşleşen kullanıcı yok.\nUserGuid: {userGuid}");
            }

            var model = _context.Orders
                .Where(o => o.AppUserId == user.Id)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(model);
        }

        public IActionResult SignIn()
        {
            return View();
        }


        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(AppUser appUser)
        {
            if (ModelState.IsValid)
            {
                appUser.CreateDate = DateTime.Now;
                appUser.UserGuid = Guid.NewGuid();
                appUser.IsActive = true;
                appUser.IsAdmin = false;

                await _context.AddAsync(appUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(SignIn));
            }

            return View(appUser);
        }

        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("SignIn");
        }


        public async Task<IActionResult> PasswordRenew()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> PasswordRenewAsync(string Email)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ModelState.AddModelError("", "Email Boş Geçilemez!");
                return View();
            }

            AppUser user = await _context.AppUsers.FirstOrDefaultAsync(x => x.Email == Email);

            if (user is null)
            {
                ModelState.AddModelError("", "Girdiğiniz Email Bulunamadı!");
                return View();
            }

            string mesaj = $"Sayın {user.Name} {user.Surname},<br>Şifrenizi yenilemek için lütfen " +
                           $"<a href='http://localhost:5109/Account/PasswordChange?user={user.UserGuid}'>buraya tıklayın</a>.";

            var sonuc = await MailHelper.SendMailAsync(Email, "Şifremi Yenile", mesaj);

            if (sonuc)
            {
                TempData["Message"] = @"<div class='alert alert-success alert-dismissible fade show' role='alert'>
            <strong>Şifre sıfırlama bağlantınız mail adresinize gönderilmiştir!</strong>
            <button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>
        </div>";
            }
            else
            {
                TempData["Message"] = @"<div class='alert alert-danger alert-dismissible fade show' role='alert'>
            <strong>Şifre sıfırlama bağlantınız gönderilemedi!</strong>
            <button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>
        </div>";
            }

            return View(); // ✅ her durumda dönüş var
        }

        public async Task<IActionResult> PasswordChangeAsync(string user)
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                return BadRequest("Geçersiz istek! Kullanıcı bilgisi eksik.");
            }

            // user parametresi muhtemelen bir UserGuid string’i
            var appUser = await _context.AppUsers
                .FirstOrDefaultAsync(x => x.UserGuid.ToString() == user);

            if (appUser is null)
            {
                return NotFound("Geçersiz Değer!");
            }

            // İsteğe bağlı: View'a appUser'ı model olarak da gönderebilirsin
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> PasswordChange(string user, string Password)
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                return BadRequest("Geçersiz istek! Kullanıcı bilgisi eksik.");
            }

            // user parametresi muhtemelen bir UserGuid string’i
            var appUser = await _context.AppUsers
                .FirstOrDefaultAsync(x => x.UserGuid.ToString() == user);

            if (appUser is null)
            {
                ModelState.AddModelError("", "Geçersiz Değer!");
                return View();
            }
            appUser.Password = Password;
            var sonuc = await _context.SaveChangesAsync();
            if (sonuc > 0)
            {
                TempData["Message"] = @"<div class='alert alert-success alert-dismissible fade show' role='alert'>
                    <strong>Şifreniz Başarıyla Değiştirilmiştir! Giriş Ekranından Oturum Açabilirsiniz.</strong> 
                    <button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>
                </div>";

            }
            else
            {
                ModelState.AddModelError("", "Güncelleme Başarısız!");
            }

            // İsteğe bağlı: View'a appUser'ı model olarak da gönderebilirsin
            return View();
        }


    }


}

