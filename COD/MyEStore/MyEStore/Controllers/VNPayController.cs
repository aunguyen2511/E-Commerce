using Microsoft.AspNetCore.Mvc;
using MyEStore.Entities;
using MyEStore.Models.Services;

namespace MyEStore.Controllers
{
	public class VNPayController : Controller
	{
		private readonly MyeStoreContext _context;
		private readonly IVnPayService _vnPayService;

		public VNPayController(MyeStoreContext context, IVnPayService vnPayService)
		{
			_context = context;
			_vnPayService = vnPayService;
		}
		public IActionResult Index()
		{
			return View();
		}
	}
}
