using AgileCommercee.Entities;
using AgileCommercee.Models;
using AgileCommercee.Models.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgileCommercee.Controllers
{
    [Authorize]
    public class HangHoasController : Controller
    {
        public readonly MyEstoreContext _context;
        public HangHoasController(MyEstoreContext context)
        {
            _context = context;
        } //sửa lại Index và thêm cái HangHoaVM


        public IActionResult Index()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            ViewBag.Loais = new SelectList(_context.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.NhaCungCaps = new SelectList(_context.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("loi", "Còn lỗi");
            }
            var data = _context.HangHoas != null ? _context.HangHoas.ToList() : new List<HangHoa>();
            return View(data);
        }

        public IActionResult IndexCus()
        {
            ViewBag.Loais = new SelectList(_context.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.NhaCungCaps = new SelectList(_context.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("loi", "Còn lỗi");
            }
            var data = _context.HangHoas != null ? _context.HangHoas.ToList() : new List<HangHoa>();
            return View(data);
        }

        [HttpGet("san-pham/{slug}")]
        public async Task<IActionResult> MoreDetails(string slug)
        {
            var products = await _context.HangHoas.ToListAsync();
            var product = products.FirstOrDefault(m => m.TenHh.ToSlug() == slug);
            if (product == null)
            {
                return NotFound();
            }
            return View("DetailsCus", product);
        }
        [HttpGet("san-phamAD/{slug}")]
        public async Task<IActionResult> MoreDetailsAD(string slug)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var products = await _context.HangHoas.ToListAsync();
            var product = products.FirstOrDefault(m => m.TenHh.ToSlug() == slug);
            if (product == null)
            {
                return NotFound();
            }
            return View("Details", product);
        }

        public IActionResult Product()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            return RedirectToAction("Index");
        }
        #region Create HangHoa
        // GET: HangHoas/Create
        [HttpGet]
        public IActionResult Create()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            ViewBag.Loais = new SelectList(_context.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.NhaCungCaps = new SelectList(_context.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            return View();
        }
        // POST: HangHoas/Create
        [HttpPost]
        public IActionResult Create(HangHoa model, IFormFile Hinh)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("loi", "Còn lỗi");
            }
            ViewBag.Loais = new SelectList(_context.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.NhaCungCaps = new SelectList(_context.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            try
            {
                //upload field logo
                model.Hinh = MyTool.UploadImageToFolder(Hinh, "HangHoas");
                _context.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
            }
            return View();
        }
		#endregion
		public IActionResult Search(string? query)
		{
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Customer")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var hangHoas = _context.HangHoas.AsQueryable();

			if (query != null)
			{
				hangHoas = hangHoas.Where(p => p.TenHh.Contains(query));
			}

			var result = hangHoas.Select(p => new HangHoa
			{
				MaHh = p.MaHh,
				TenHh = p.TenHh,
                TenAlias = p.TenAlias,
				DonGia = p.DonGia ?? 0,
				Hinh = p.Hinh ?? "",
				MoTa = p.MoTaDonVi ?? "",
				MaLoai = p.MaLoaiNavigation.MaLoai
			});
			return View(result);
		}

        public IActionResult SearchAD(string? query)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var hangHoas = _context.HangHoas.AsQueryable();

            if (query != null)
            {
                hangHoas = hangHoas.Where(p => p.TenHh.Contains(query));
            }

            var result = hangHoas.Select(p => new HangHoa
            {
                MaHh = p.MaHh,
                TenHh = p.TenHh,
                TenAlias = p.TenAlias,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTa = p.MoTaDonVi ?? "",
                MaLoai = p.MaLoaiNavigation.MaLoai
            });
            return View(result);
        }

        #region Details Hang Hoa
        public async Task<IActionResult> Details(int? id)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
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

        public async Task<IActionResult> DetailsCus(int? id)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Customer")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
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

        // GET: HangHoas/Edit
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            ViewBag.Loais = new SelectList(_context.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.NhaCungCaps = new SelectList(_context.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            var existedProduct = _context.HangHoas.SingleOrDefault(x => x.MaHh == id);
            if (existedProduct != null)
            {
                return View(existedProduct);
            }
            return NotFound();
        }
        // POST: HangHoas/Edit
        [HttpPost]
        public IActionResult Edit(HangHoa modelEdit, IFormFile Hinh)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            ViewBag.Loais = new SelectList(_context.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.NhaCungCaps = new SelectList(_context.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            var existedProduct = _context.HangHoas.SingleOrDefault(x => x.MaHh == modelEdit.MaHh);
            try
            {
                //Edit
                existedProduct.TenHh = modelEdit.TenHh;
                existedProduct.MoTa = modelEdit.MoTa;
                existedProduct.MaLoai = modelEdit.MaLoai;
                existedProduct.DonGia = modelEdit.DonGia;
                existedProduct.NgaySx = modelEdit.NgaySx;
                existedProduct.GiamGia = modelEdit.GiamGia;
                existedProduct.SoLanXem = modelEdit.SoLanXem;
                existedProduct.MoTa = modelEdit.MoTa;
                existedProduct.MaNcc = modelEdit.MaNcc;

                if (Hinh == null)
                {
                    existedProduct.Hinh = modelEdit.Hinh;
                }
                else
                {
                    existedProduct.Hinh = MyTool.UploadImageToFolder(Hinh, "HangHoa");
                }
                //Save
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
            }
            return View();
        }

        [HttpGet]
        public IActionResult Delete(int Id)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var product = _context.HangHoas.SingleOrDefault(x => x.MaHh == Id);
            if(product != null)
            {
                return View(product);
            }
            return NotFound();
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmDelete(int Id) 
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var product = _context.HangHoas.SingleOrDefault(x => x.MaHh == Id);
            if(product != null)
            {
                _context.HangHoas.Remove(product);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        #region Statistic By Category
        [HttpGet("/Statistic/ByCategory")]
        public IActionResult StatisticByCategory()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var data = _context.HangHoas
                .GroupBy(p => new
                {
                    MaLoaiNavigation = p.MaLoaiNavigation.TenLoai
                })
                .Select(g => new CategoryStatistic
                {
                    MaLoaiNavigation = g.Key.MaLoaiNavigation,
                    NumOfProduct = g.Count()
                }).ToList();
            return View(data);
        }
        #endregion Statistic By Category

        #region Statistic By Supplier
        [HttpGet("/Statistic/BySupplier")]
        public IActionResult StatisticBySupplier()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var data = _context.HangHoas
                .GroupBy(p => new
                {
                    MaNccNavigation = p.MaNccNavigation.TenCongTy
                })
                .Select(g => new SupplierStatistic
                {
                    MaNccNavigation = g.Key.MaNccNavigation,
                    NumOfProduct = g.Count()
                }).ToList();
            return View(data);
        }
        #endregion Statistic By Supplier

        #region Statistic By Supplier & Category
        [HttpGet("/Statistic/BySupplierCategory")]
        public IActionResult StatisticBySupplierCategory()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var data = _context.HangHoas
                .GroupBy(p => new
                {
                    MaLoaiNavigation = p.MaLoaiNavigation.TenLoai,
                    MaNccNavigation = p.MaNccNavigation.TenCongTy
                })
                .Select(g => new SupplierCategoryStatistic
                {
                    MaLoaiNavigation = g.Key.MaLoaiNavigation,
                    MaNccNavigation = g.Key.MaNccNavigation,
                    NumOfProduct = g.Count()
                }).ToList();
            return View(data);
        }
        #endregion Statistic By Supplier & Category
    }
}

