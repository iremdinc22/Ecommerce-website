using System.Runtime.InteropServices;
using Eticaret.Core.Entities;
using Eticaret.Data;
using Eticaret.Service.Abstract;
using Eticaret.Service.Concrete;
using Eticaret.WebUI.ExtensionMethods;
using Eticaret.WebUI.Models;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eticaret.WebUI.Controllers
{
    public class CartController : Controller
    {
        private readonly DatabaseContext _db;
        private readonly IService<Product> _serviceProduct;
        private readonly IService<Core.Entities.Address> _serviceAddress;
        private readonly IService<AppUser> _serviceAppUser;
        private readonly IConfiguration _configuration;

        public CartController(
            DatabaseContext db,
            IService<Product> serviceProduct,
            IService<Core.Entities.Address> serviceAddress,
            IService<AppUser> serviceAppUser,
            IConfiguration configuration)
        {
            _db = db;
            _serviceProduct = serviceProduct;
            _serviceAddress = serviceAddress;
            _serviceAppUser = serviceAppUser;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            var model = new CartViewModel
            {
                CartLines = cart.CartLines,
                TotalPrice = cart.TotalPrice()
            };
            return View(model);
        }

        public IActionResult Add(int ProductId, int quantity = 1)
        {
            var product = _serviceProduct.Find(ProductId);
            if (product != null)
            {
                var cart = GetCart();
                cart.AddProduct(product, quantity);
                HttpContext.Session.SetJson("Cart", cart);
                return Redirect(Request.Headers["Referer"].ToString());
            }
            return RedirectToAction("Index");
        }

        public IActionResult Update(int ProductId, int quantity = 1)
        {
            var product = _serviceProduct.Find(ProductId);
            if (product != null)
            {
                var cart = GetCart();
                cart.UpdateProduct(product, quantity);
                HttpContext.Session.SetJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int ProductId)
        {
            var product = _serviceProduct.Find(ProductId);
            if (product != null)
            {
                var cart = GetCart();
                cart.RemoveProduct(product);
                HttpContext.Session.SetJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<IActionResult> CheckoutAsync()
        {
            var cart = GetCart();
            var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);

            if (appUser == null)
                return RedirectToAction("SignIn", "Account");

            var addresses = await _serviceAddress.GetAllAsync(a => a.AppUserId == appUser.Id && a.IsActive);

            var model = new CheckoutViewModel
            {
                CartProducts = cart.CartLines,
                TotalPrice = cart.TotalPrice(),
                Addresses = addresses
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var userGuid = HttpContext.User.FindFirst("UserGuid")?.Value;
            var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == userGuid);

            if (appUser == null)
                return RedirectToAction("SignIn", "Account");

            var cart = GetCart();

            if (cart == null || cart.CartLines.Count == 0)
                ModelState.AddModelError("", "Sepetiniz boş. Sipariş oluşturmak için ürün ekleyiniz.");

            if (string.IsNullOrEmpty(model.DeliveryAddress))
                ModelState.AddModelError(nameof(model.DeliveryAddress), "Teslimat adresi seçilmelidir.");

            if (string.IsNullOrEmpty(model.BillingAddress))
                ModelState.AddModelError(nameof(model.BillingAddress), "Fatura adresi seçilmelidir.");

            //if (!ModelState.IsValid)
            // {
            // model.CartProducts = cart.CartLines;
            // model.TotalPrice = cart.TotalPrice();
            // model.Addresses = await _serviceAddress.GetAllAsync(x => x.AppUserId == appUser.Id && x.IsActive);
            //return View(model);
            // }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState geçersiz");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"Hata: {key} => {error.ErrorMessage}");
                    }
                }

                model.CartProducts = cart.CartLines;
                model.TotalPrice = cart.TotalPrice();
                model.Addresses = await _serviceAddress.GetAllAsync(x => x.AppUserId == appUser.Id && x.IsActive);
                return View(model);
            }


            var addresses = await _serviceAddress.GetAllAsync(a => a.AppUserId == appUser.Id && a.IsActive);
            var faturaAdresi = addresses.FirstOrDefault(a => a.AddressGuid.ToString() == model.BillingAddress);
            var teslimatAdresi = addresses.FirstOrDefault(a => a.AddressGuid.ToString() == model.DeliveryAddress);

            Order siparis;

            try
            {
                siparis = new Order
                {
                    AppUserId = appUser.Id,
                    BillingAddress = $"{faturaAdresi?.OpenAddress}, {faturaAdresi?.District}, {faturaAdresi?.City}",
                    DeliveryAddress = $"{teslimatAdresi?.OpenAddress}, {teslimatAdresi?.District}, {teslimatAdresi?.City}",
                    CustomerId = appUser.UserGuid.ToString(),
                    OrderDate = DateTime.Now,
                    TotalPrice = cart.TotalPrice(),
                    OrderNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    OrderState = 0,
                    OrderLines = []
                };

                _db.Orders.Add(siparis);
                await _db.SaveChangesAsync();



                #region OdemeIslemi
                Options options = new Options();
                options.ApiKey = _configuration["IyzicOptions:ApiKey"];
                options.SecretKey = _configuration["IyzicOptions:SecretKey"];
                options.BaseUrl = _configuration["IyzicOptions:BaseUrl"];

                CreatePaymentRequest request = new CreatePaymentRequest();
                request.Locale = Locale.TR.ToString();
                request.ConversationId = HttpContext.Session.Id;
                request.Price = siparis.TotalPrice.ToString().Replace(",", ".");
                request.PaidPrice = siparis.TotalPrice.ToString().Replace(",", ".");
                request.Currency = Currency.TRY.ToString();
                request.Installment = 1;
                request.BasketId = "B" + HttpContext.Session.Id;
                request.PaymentChannel = PaymentChannel.WEB.ToString();
                request.PaymentGroup = PaymentGroup.PRODUCT.ToString();

                PaymentCard paymentCard = new PaymentCard();
                paymentCard.CardHolderName = model.CardHolderName;   // "John Doe";
                paymentCard.CardNumber = model.CardNumber;  //"5528790000000008";
                paymentCard.ExpireMonth = model.CardMonth;//"12";
                paymentCard.ExpireYear = model.CardYear;// "2030";
                paymentCard.Cvc = model.CVV; //"123";
                paymentCard.RegisterCard = 0;
                request.PaymentCard = paymentCard;

                Buyer buyer = new Buyer();
                buyer.Id = "BY" + appUser.Id;
                buyer.Name = appUser.Name;
                buyer.Surname = appUser.Surname;
                buyer.GsmNumber = appUser.Phone;
                buyer.Email = appUser.Email;
                buyer.IdentityNumber = "11111111111";
                buyer.LastLoginDate = DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss"); // "2015-10-05 12:43:35"
                buyer.RegistrationDate = appUser.CreateDate.ToString("yyyy-mm-dd hh:mm:ss"); // "2013-04-21 15:12:09"
                buyer.RegistrationAddress = siparis.DeliveryAddress;
                buyer.Ip = HttpContext.Connection.RemoteIpAddress?.ToString(); // "85.34.78.112"
                buyer.City = teslimatAdresi.City;
                buyer.Country = "Turkey";
                buyer.ZipCode = "34732";
                request.Buyer = buyer;


                var shippingAddress = new Iyzipay.Model.Address();
                shippingAddress.ContactName = appUser.Name + " " + appUser.Surname;
                shippingAddress.City = teslimatAdresi.City;
                shippingAddress.Country = "Turkey";
                shippingAddress.Description = teslimatAdresi.OpenAddress;
                shippingAddress.ZipCode = "34742";
                request.ShippingAddress = shippingAddress;

                var billingAddress = new Iyzipay.Model.Address();
                billingAddress.ContactName = appUser.Name + " " + appUser.Surname;
                billingAddress.City = faturaAdresi.City;
                billingAddress.Country = "Turkey";
                billingAddress.Description = faturaAdresi.OpenAddress;
                billingAddress.ZipCode = "34742";
                request.BillingAddress = billingAddress;

                // Sipariş kalemlerini OrderLine'a ekle ve aynı zamanda BasketItem listesi oluştur
                List<BasketItem> basketItems = new List<BasketItem>();


                foreach (var item in cart.CartLines)
                {
                    _db.OrderLines.Add(new OrderLine
                    {
                        OrderId = siparis.Id,
                        ProductId = item.Product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price
                    });
                    basketItems.Add(new BasketItem
                    {
                        Id = item.Product.Id.ToString(),
                        Name = item.Product.Name,
                        Category1 = "Collectibles",
                        ItemType = BasketItemType.PHYSICAL.ToString(),
                        Price = (item.Product.Price * item.Quantity).ToString().Replace(",", ".")
                    });


                }

                if (siparis.TotalPrice > 999)
                {
                    basketItems.Add(new BasketItem
                    {
                        Id = "Kargo",
                        Name = "Kargo Ücreti",
                        Category1 = "Kargo Ücreti",
                        ItemType = BasketItemType.VIRTUAL.ToString(),
                        Price = "99"
                    });
                    siparis.TotalPrice += 99;
                    request.Price = siparis.TotalPrice.ToString().Replace(",", ".");
                    request.PaidPrice = siparis.TotalPrice.ToString().Replace(",", ".");

                }

                request.BasketItems = basketItems;
                Payment payment = await Payment.Create(request, options);
                Console.WriteLine("Ödeme durumu: " + payment.Status);
                Console.WriteLine("Ödeme mesajı: " + payment.ErrorMessage);

                #endregion

                try
                {
                    if (payment.Status?.ToLower() == "success")

                    {
                        // sipariş oluştur 
                        var sonuc = await _db.SaveChangesAsync();

                        if (sonuc > 0)
                        {
                            HttpContext.Session.Remove("Cart");
                            return RedirectToAction("Thanks");
                        }
                    }
                    else
                    {
                        TempData["Error"] = $"<div class='alert alert-danger'>Ödeme İşlemi Başarısız!</div> ({payment.ErrorMessage})";

                    }

                }
                catch (Exception ex)
                {
                    TempData["Error"] = "<div class='alert alert-danger'>Hata Oluştu!</div>";
                }

                model.CartProducts = cart.CartLines;
                model.TotalPrice = cart.TotalPrice();
                model.Addresses = await _serviceAddress.GetAllAsync(x => x.AppUserId == appUser.Id && x.IsActive);
                return View(model);
            } // try bloğu kapanışı
            catch (Exception ex)
            {
                TempData["Error"] = "Sipariş sırasında hata: " + ex.Message;
                model.CartProducts = cart.CartLines;
                model.TotalPrice = cart.TotalPrice();
                model.Addresses = await _serviceAddress.GetAllAsync(x => x.AppUserId == appUser.Id && x.IsActive);
                return View(model);
            }
        } // Checkout metodu kapanışı

        public IActionResult Thanks()
        {
            return View();
        }

        private CartService GetCart()
        {
            return HttpContext.Session.GetJson<CartService>("Cart") ?? new CartService();
        }
    } // CartController sınıf kapanışı
} // namespace kapanışı


