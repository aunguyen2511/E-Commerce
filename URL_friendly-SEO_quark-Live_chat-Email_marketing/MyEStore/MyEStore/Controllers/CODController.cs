using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEStore.Entities;

namespace MyEStore.Controllers
{
	public class CODController : Controller
	{
		private readonly MyeStoreContext _context;

		public CODController(MyeStoreContext context) 
		{
			_context = context;
		}
		public IActionResult Index()
		{
			return View();
		}
	}
}
