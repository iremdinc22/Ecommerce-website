using Eticaret.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class MainController : Controller
    {
        private readonly DatabaseContext _context;

        public MainController(DatabaseContext context)
        {
            _context = context;
        }

        //public IActionResult Index()
        //{
        //ViewBag.Products = _context.Products;
        //return View();
        //}
        public IActionResult Index()
        {
            ViewBag.Products = _context.Products.ToList();
            ViewBag.Users = _context.AppUsers.Count();
            ViewBag.Orders = _context.Orders.Count();

            ViewBag.TodaySales = _context.Orders
                .Where(o => o.OrderDate.Date == DateTime.Today)
                .AsEnumerable()
                .Sum(o => o.TotalPrice);

            ViewBag.TodayOrders = _context.Orders
                .Count(o => o.OrderDate.Date == DateTime.Today);

            return View();
        }





    }
}
