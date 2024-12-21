using AgileCommercee.Entities;
using AgileCommercee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgileCommercee.Controllers
{
    public class LoaisController : Controller
    {
        private readonly MyEstoreContext _Context;
        public LoaisController(MyEstoreContext context)
        {
            _Context = context;
        }
        public IActionResult Index()
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("loi", "Còn lỗi");
            }
            var data = _Context.Loais != null ? _Context.Loais.ToList() : new List<Loai>();
            return View(data);
        }

        public IActionResult IndexCus()
        {
            if(!ModelState.IsValid)
            {
                ModelState.AddModelError("loi", "Còn lỗi");
            }
            var data = _Context.Loais != null ? _Context.Loais.ToList() : new List<Loai>();
            return View(data);
        }

        // GET: Loai/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Loai/Create
        [HttpPost]
        public IActionResult Create(Loai model, IFormFile FileLogo)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("loi", "Còn lỗi");
            }
            /*
            if (FileLogo != null)
            {
                model.Hinh = MyTool.UploadImageToFolder(FileLogo, "Hinh");
            }
            */
            try
            {
                model.Hinh = MyTool.UploadImageToFolder(FileLogo, "Loais");
                _Context.Add(model);
                _Context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
            }
            return View();
        }

        // GET: Loai/Delete
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var loai = _Context.Loais.SingleOrDefault(x => x.MaLoai == id);
            if (loai != null)
            {
                return View(loai);
            }
            return NotFound();
        }

        // POST: Loai/ConfirmDelete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmDelete(int id)
        {
            var loai = _Context.Loais.SingleOrDefault(x => x.MaLoai == id);
            if (loai != null)
            {
                _Context.Loais.Remove(loai);
                _Context.SaveChanges();
                return RedirectToAction("Index");
            }
            return NotFound();
        }
        // GET: Loais/Edit
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var existedCategory = _Context.Loais.SingleOrDefault(x => x.MaLoai == id);
            if (existedCategory != null)
            {
                return View(existedCategory);
            }
            return NotFound();
        }
        // POST: Loais/Edit
        [HttpPost]
        public IActionResult Edit(Loai modelEdit, IFormFile FileLogo)
        {
            var existedCategory = _Context.Loais.SingleOrDefault(x => x.MaLoai == modelEdit.MaLoai);
            try
            {
                //Edit
                existedCategory.TenLoai = modelEdit.TenLoai;
                existedCategory.MoTa = modelEdit.MoTa;
                if (FileLogo == null)
                {
                    existedCategory.Hinh = modelEdit.Hinh;
                }
                else
                {
                    existedCategory.Hinh = MyTool.UploadImageToFolder(FileLogo, "Loais");
                }
                //Save
                _Context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
            }
            return View();
        }
        // GET: Loais/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _Context.Loais.FirstOrDefaultAsync(m => m.MaLoai == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        #region Get Product By Category
        [HttpGet]
        public IActionResult GetProductByCategories(int maloai)
        {
            var data = _Context.HangHoas
                .Where(p => p.MaLoai == maloai)
                .Select(p => new ProductByCategoryVM
                {
                    HangHoaMaHh = p.MaHh,
                    HangHoaTenHh = p.TenHh,
                    DonGia = p.DonGia,
                    MaLoaiNavigation = p.MaLoaiNavigation.TenLoai,
                    MaNccNavigation = p.MaNccNavigation.TenCongTy
                }).ToList();
            if (data == null || !data.Any())
            {
                return NotFound();
            }
            return View(data);
        }
        #endregion Get Product By Category

        // GET: Loais/Details/5
        public async Task<IActionResult> DetailsCus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _Context.Loais.FirstOrDefaultAsync(m => m.MaLoai == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        #region Get Product By Category
        [HttpGet]
        public IActionResult GetProductByCategoriesCus(int maloai)
        {
            var data = _Context.HangHoas
                .Where(p => p.MaLoai == maloai)
                .Select(p => new ProductByCategoryVM
                {
                    HangHoaMaHh = p.MaHh,
                    HangHoaTenHh = p.TenHh,
                    DonGia = p.DonGia,
                    MaLoaiNavigation = p.MaLoaiNavigation.TenLoai,
                    MaNccNavigation = p.MaNccNavigation.TenCongTy
                }).ToList();
            if (data == null || !data.Any())
            {
                return NotFound();
            }
            return View(data);
        }
        #endregion Get Product By Category
    }
}
