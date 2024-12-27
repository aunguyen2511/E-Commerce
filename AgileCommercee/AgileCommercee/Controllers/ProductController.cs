using AgileCommercee.Entities;
using AgileCommercee.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace AgileCommercee.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly MyEstoreContext _context;
        private readonly IDictionary<string, IStatisticsStrategy> _strategies;

        public ProductsController(MyEstoreContext context)
        {
            _context = context;
            _strategies = new Dictionary<string, IStatisticsStrategy>
        {
            { "category", new CategoryStatisticsStrategy() },
            { "supplier", new SupplierStatisticsStrategy() },
            { "combined", new SupplierCategoryStatisticStrategy() }
        };
        }

        public IActionResult Statistics(string type)
        {

            if (string.IsNullOrEmpty(type))
            {
                return BadRequest("Statistic type is required.");
            }

            var products = GetProducts();

            if (_strategies.TryGetValue(type, out var strategy))
            {
                var statistics = strategy.GetStatistics(products);
                if (statistics == null || !statistics.Any())
                {
                    return NotFound("No statistics available.");
                }
                return View(statistics);
            }

            return BadRequest("Invalid statistic type.");
        }
        private IEnumerable<HangHoa> GetProducts()
        {
            var products = _context.HangHoas.AsNoTracking().ToList();
            Console.WriteLine($"Total products retrieved: {products.Count}");
            return _context.HangHoas.AsNoTracking().ToList();
        }
    }
}
