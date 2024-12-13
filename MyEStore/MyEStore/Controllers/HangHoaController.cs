using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using MyEStore.Entities;
using MyEStore.Models;

namespace MyEStore.Controllers
{
    public class HangHoasController : Controller
    {
        private readonly MyeStoreContext _context;
        public HangHoasController(MyeStoreContext context)
        {
            _context = context;
        }

        #region Index Hang Hoa
        public IActionResult Index(int? cateId)
        {
            //ViewBag.Loais = new SelectList(_context.Loais.ToList(), "MaLoai", "TenLoai");
            //ViewBag.NhaCungCaps = new SelectList(_context.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            //if (!ModelState.IsValid)
            //{
            //    ModelState.AddModelError("loi", "Còn lỗi");
            //}
            var data = _context.HangHoas.AsQueryable();
            if (cateId.HasValue)
            {
                data = data.Where(hh => hh.MaLoai == cateId.Value);
            }
            var result = data.Select(hh => new HangHoaVM{
                MaHh = hh.MaHh,
                TenHh = hh.TenHh,
                DonGia = hh.DonGia ?? 0,
                Hinh = hh.Hinh
            }).ToList();
            return View(result);
        }
        #endregion Index Hang Hoa

        #region Details Hang Hoa
        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.Loais = new SelectList(_context.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.NhaCungCaps = new SelectList(_context.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            if (id == null)
            {
                return NotFound();
            }
            var product = await _context.HangHoas.FirstOrDefaultAsync(m => m.MaHh == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        #endregion Details Hang Hoa

        

    }
}
