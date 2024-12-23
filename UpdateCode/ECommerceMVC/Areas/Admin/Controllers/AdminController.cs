using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using ECommerceMVC.Helpers;
using Microsoft.AspNetCore.Authentication;
using X.PagedList.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly IMapper _mapper;
        private readonly Hshop2023Context _context;

        public AdminController(IMapper mapper, Hshop2023Context context)
        {
            _mapper = mapper;
            _context = context;
        }

        #region Start Login Admin

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.KhachHangs.SingleOrDefault(kh => kh.MaKh == model.UserName);
            if (user == null || !user.HieuLuc)
            {
                ModelState.AddModelError("loi", "Sai tên đăng nhập hoặc tài khoản đã bị khóa.");
                return View(model);
            }

            var hashedPassword = model.Password.ToMd5Hash(user.RandomKey);
            if (user.MatKhau != hashedPassword)
            {
                ModelState.AddModelError("loi", "Sai thông tin đăng nhập.");
                return View(model);
            }

            var userImage = Url.Content($"~/{user.Hinh}".Trim('/'));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.HoTen),
                new Claim("UserImage", userImage), // Đảm bảo URL đã chuẩn hóa
                new Claim(ClaimTypes.Role, "Admin")
            };

            Console.WriteLine($"UserImage URL: {userImage}");

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ManageUsers()
        {
            var users = _context.KhachHangs.ToList();
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        #endregion

        #region Product CRUD
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Loais, "MaLoai", "TenLoai");
            ViewBag.Suppliers = new SelectList(_context.NhaCungCaps, "MaNcc", "TenCongTy");
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(HangHoaVM model, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
                // Handle the file upload
                string fileName = null;
                if (Hinh != null && Hinh.Length > 0)
                {
                    fileName = Path.GetFileName(Hinh.FileName);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Hinh/HangHoa", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Hinh.CopyToAsync(stream);
                    }
                }

                var product = new HangHoa
                {
                    TenHh = model.TenHh,
                    TenAlias = model.TenAlias,
                    MaLoai = model.MaLoai,
                    MoTaDonVi = model.MoTaDonVi,
                    DonGia = model.DonGia,
                    Hinh = fileName, // Save the file name to the database
                    NgaySx = model.NgaySx,
                    GiamGia = model.GiamGia,
                    MoTa = model.MoTa,
                    MaNcc = model.MaNcc,
                    SoLanXem = 0
                };

                _context.HangHoas.Add(product);
                _context.SaveChanges();
                return RedirectToAction("ManageProducts");
            }

            ViewBag.Categories = new SelectList(_context.Loais, "MaLoai", "TenLoai", model.MaLoai);
            ViewBag.Suppliers = new SelectList(_context.NhaCungCaps, "MaNcc", "TenCongTy", model.MaNcc);
            return View(model);
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var product = _context.HangHoas.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            var model = new HangHoaVM
            {
                MaHh = product.MaHh,
                TenHh = product.TenHh,
                TenAlias = product.TenAlias,
                MaLoai = product.MaLoai,
                MoTaDonVi = product.MoTaDonVi,
                DonGia = product.DonGia,
                Hinh = product.Hinh,
                NgaySx = product.NgaySx,
                GiamGia = product.GiamGia,
                MoTa = product.MoTa,
                MaNcc = product.MaNcc
            };

            // Set ViewData and ViewBag before returning the view
            ViewData["Title"] = "Edit Product";
            ViewBag.Categories = new SelectList(_context.Loais, "MaLoai", "TenLoai", model.MaLoai);
            ViewBag.Suppliers = new SelectList(_context.NhaCungCaps, "MaNcc", "TenCongTy", model.MaNcc);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(HangHoaVM model, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
                var product = _context.HangHoas.Find(model.MaHh);
                if (product == null)
                {
                    return NotFound();
                }

                product.TenHh = model.TenHh;
                product.TenAlias = model.TenAlias;
                product.MaLoai = model.MaLoai;
                product.MoTaDonVi = model.MoTaDonVi;
                product.DonGia = model.DonGia;
                product.NgaySx = model.NgaySx;
                product.GiamGia = model.GiamGia;
                product.MoTa = model.MoTa;
                product.MaNcc = model.MaNcc;

                // Kiểm tra file ảnh mới
                if (Hinh != null && Hinh.Length > 0)
                {
                    var fileName = Path.GetFileName(Hinh.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Hinh/HangHoa", fileName);
                    Console.WriteLine($"Saving image to {filePath}");

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        Hinh.CopyTo(stream);
                    }

                    product.Hinh = fileName;
                }

                _context.SaveChanges();
                return RedirectToAction("ManageProducts");
            }

            // Ghi nhật ký lỗi
            foreach (var modelState in ModelState)
            {
                Console.WriteLine($"{modelState.Key}: {modelState.Value.Errors.FirstOrDefault()?.ErrorMessage}");
            }

            // Set ViewData and ViewBag before returning the view in case of errors
            ViewData["Title"] = "Edit Product";
            ViewBag.Categories = new SelectList(_context.Loais, "MaLoai", "TenLoai", model.MaLoai);
            ViewBag.Suppliers = new SelectList(_context.NhaCungCaps, "MaNcc", "TenCongTy", model.MaNcc);
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var product = _context.HangHoas.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            var model = new HangHoaVM
            {
                MaHh = product.MaHh,
                TenHh = product.TenHh,
                TenAlias = product.TenAlias,
                MaLoai = product.MaLoai,
                MoTaDonVi = product.MoTaDonVi,
                DonGia = product.DonGia,
                Hinh = product.Hinh,
                NgaySx = product.NgaySx,
                GiamGia = product.GiamGia,
                MoTa = product.MoTa,
                MaNcc = product.MaNcc
            };

            return View(model); // Đảm bảo rằng bạn truyền đúng kiểu model
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(HangHoaVM model)
        {
            var product = _context.HangHoas.Find(model.MaHh);
            if (product == null)
            {
                return NotFound();
            }

            _context.HangHoas.Remove(product);
            _context.SaveChanges();

            return RedirectToAction("ManageProducts");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Details(int id)
        {
            var product = _context.HangHoas
                                  .Include(h => h.MaLoaiNavigation) // Include related data if needed
                                  .FirstOrDefault(h => h.MaHh == id);
            if (product == null)
            {
                return NotFound();
            }

            var model = new ChiTietHangHoaVM
            {
                MaHh = product.MaHh,
                TenHH = product.TenHh,
                Hinh = product.Hinh,
                DonGia = product.DonGia ?? 0,
                MoTaNgan = product.MoTa,
                TenLoai = product.MaLoaiNavigation.TenLoai,
                ChiTiet = product.MoTa,
                DiemDanhGia = 0, // Placeholder for ratings
                SoLuongTon = 0  // Placeholder for stock quantity
            };

            return View(model); // Ensure the view file is located in Views/Admin/Details.cshtml
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ManageProducts(int page = 1, int pageSize = 9)
        {
            var products = _context.HangHoas
                .Include(h => h.MaLoaiNavigation) // Ensure related category data is loaded
                .ToPagedList(page, pageSize);
            return View(products); // Return to ManageProducts view
        }
        #endregion
    }
}
