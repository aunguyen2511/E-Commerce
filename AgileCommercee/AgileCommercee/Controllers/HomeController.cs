using AgileCommercee.Entities;
using AgileCommercee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AgileCommercee.Controllers
{
    public class HomeController : Controller
    {
        public readonly MyEstoreContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, MyEstoreContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var products = _context.HangHoas.ToList();
            if (products == null || !products.Any())
            {
                ViewBag.Products = new List<HangHoa>(); // Hoặc gán danh sách rỗng
            }
            else
            {
                ViewBag.Products = products;
            }
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
