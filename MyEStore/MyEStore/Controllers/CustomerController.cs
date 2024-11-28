using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MyEStore.Entities;
using MyEStore.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MyEStore.Controllers
{
    public class CustomerController : Controller
    {
        private readonly MyeStoreContext _context;
        public CustomerController(MyeStoreContext context)
        {
            _context = context;
        }

        #region Customer - Register (Đăng ký)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
            // làm sau cái Login :((
        }

        [HttpPost]
        public IActionResult Register(RegisterVM model, IFormFile FileHinh)
        {
            try
            {
                var khachHang = new KhachHang
                {
                    MaKh = model.MaKh,
                    HoTen = model.HoTen,
                    NgaySinh = model.NgaySinh,
                    DiaChi = model.DiaChi,
                    GioiTinh = model.GioiTinh,
                    DienThoai = model.DienThoai,
                    Email = model.Email,
                    Hinh = MyTool.UploadImageToFolder(FileHinh, "KhachHang"),
                    HieuLuc = true, //false + gửi mail active tài khoản ????????????????????
                    RandomKey = MyTool.GetRandom()
                };

                khachHang.MatKhau = model.MatKhau.ToMd5Hash(khachHang.RandomKey);
                _context.Add(khachHang);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            catch(Exception ex)
            {
                return View();
            }
        }

        #endregion Customer - Register (Đăng ký)

        //------------------------------

        #region Customer - Login (Đăng nhập)
        [HttpGet]
        public IActionResult Login(string ReturnUrl = null)
        {
            // đăng nhập rồi thì lấy username ở đâu
            ViewBag.ReturnUrl = ReturnUrl; 
            return View();
        }

        [HttpPost]
        public async Task <IActionResult> Login(LoginVM model, string ReturnUrl = null, string ThongBao = null)
        {
            var khachHang = _context.KhachHangs.SingleOrDefault(p => p.MaKh == model.UserName);
            ViewBag.ReturnUrl = ReturnUrl;
            if (khachHang == null)
            {
                ViewBag.ThongBao = "Sai thông tin đăng nhập. Nhập lại tên người dùng";
                //TempData["ThongBao"] = "Sai thông tin đăng nhập";
                return View();
            }

            // check coi password nhập vô có khớp với password đã đc mã hoá trong DB hay ko
            if(khachHang.MatKhau != model.Password.ToMd5Hash(khachHang.RandomKey))
            {
                ViewBag.ThongBao = "Đăng nhập không thành công. Nhập lại mật khẩu";
                //TempData["ThongBao"] = "Đăng nhập không thành công";
                return View();
            }

            //khai báo claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, khachHang.Email),
                new Claim(ClaimTypes.Name, khachHang.HoTen),
                new Claim("ID", khachHang.MaKh),

                // quyền (role)
                new Claim(ClaimTypes.Role, "Administrator"),
                new Claim(ClaimTypes.Role, "Accountant"),
            };

            var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var claimPrincipal = new ClaimsPrincipal(claimsIdentity);

            // chỗ này vừa copy vô thì bị lỗi do hàm Sign In của mình ko có async, chuyển nó thành async rồi thì nó sẽ hết lỗi
            await HttpContext.SignInAsync(claimPrincipal);

            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            // Profile làm sau cái Authen
            return RedirectToAction("Profile", "Customer");
        }
        #endregion Customer - Login (Đăng nhập)

        [Authorize]
        public IActionResult PurchaseHistory()
        {
            return View();
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
    }
}

