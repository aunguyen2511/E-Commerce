using AgileCommercee.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AgileCommercee.Controllers
{
    public class SubscribeController : Controller
    {
        public readonly MyEstoreContext _context;
        public SubscribeController(MyEstoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
