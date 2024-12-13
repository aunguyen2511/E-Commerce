using Microsoft.AspNetCore.Mvc;
using MyEStore.Entities;

namespace MyEStore.Models
{
    public class LoaiViewComponent : ViewComponent
    {
        public readonly MyeStoreContext _context;
        public LoaiViewComponent(MyeStoreContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            // gửi tới ~/Views/Shared/Components/Loai/Default.cshtml
            return View(_context.Loais.ToList());
        }
    }
}
