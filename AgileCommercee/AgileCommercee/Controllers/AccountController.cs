
using AgileCommercee.Entities;
using AgileCommercee.Models;
using AgileCommercee.Models.Helper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging.Signing;
using System.Security.Claims;

namespace AgileCommercee.Controllers
{
    public class AccountsController : Controller
    {
        private readonly MyEstoreContext _context;
        public AccountsController(MyEstoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var data = _context.KhachHangs != null ? _context.KhachHangs.ToList() : new List<KhachHang>();
            return View(data);
        }
        
        public IActionResult Main()
        {
            var maKh = User.FindFirst("ID")?.Value;
            var userType = HttpContext.Session.GetString("UserType");

            // Kiểm tra giá trị của tài khoản đăng nhập theo table nào
            if (!string.IsNullOrEmpty(maKh))
            {
                if (userType == "Customer")
                {
                    return RedirectToAction("CustomerLogin");
                }
                else if (userType == "Employee")
                {
                    return RedirectToAction("AfterLogin");
                }
            }
            //{
            //    return RedirectToAction("CustomerLogin", "Accounts");
            //}
            //else if (userRoles == 0 && userId != null)
            //{
            //    return RedirectToAction("AfterLogin", "Accounts");
            //}

            var products = _context.HangHoas.ToList();
            ViewBag.Products = products;
            return View(products);
        }
        [Authorize]
        public IActionResult AfterLogin()
        {
            var userId = HttpContext.Session.GetString("ID");
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var products = _context.HangHoas.ToList();
            return View(products);
        }
        [Authorize]
        public IActionResult CustomerLogin()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Customer")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var userId = HttpContext.Session.GetString("ID");
            var products = _context.HangHoas.ToList();
            return View(products);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model, IFormFile Hinh)
        {
            if (Hinh != null)
            {
                model.Hinh = MyTool.UploadImageToFolder(Hinh, "Hinh");
            }
            try
            {
                var user = new KhachHang
                {
                    MaKh = model.UserName,
                    HoTen = model.FullName,
                    Email = model.Email,
                    DiaChi = model.Address,
                    DienThoai = model.PhoneNumber,
                    MatKhau = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Hinh = model.Hinh,
                    VaiTro = 1
                };

                _context.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Main");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return RedirectToAction("Main");
        }
        [Authorize]
        [HttpGet]
        public IActionResult RegisterAD() {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            return View(); 
        }

        [HttpPost]
        public IActionResult RegisterAD(AdminRegisterViewModel model, IFormFile Hinh)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            if (Hinh != null)
            {
                model.Hinh = MyTool.UploadImageToFolder(Hinh, "Hinh");
            }
            try
            {
                var user = new KhachHang
                {
                    MaKh = model.UserName,
                    HoTen = model.FullName,
                    Email = model.Email,
                    MatKhau = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    VaiTro = 0
                };
                _context.Add(user);
                _context.SaveChanges();
                return RedirectToAction("AfterLogin");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return RedirectToAction("AfterLogin");
        }

        [HttpGet]
        public IActionResult Login(string ReturnUrl = null) { ViewBag.ReturnUrl = ReturnUrl; return View(); }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string ReturnUrl = null, string ThongBao = null)
        {
            if (ModelState.IsValid)
            {
                var user = _context.KhachHangs.SingleOrDefault(k => k.MaKh == model.UserName);
                if (user == null)
                {
                    ViewBag.ThongBao = "Sai thông tin đăng nhập. Nhập lại tên người dùng";
                    //TempData["ThongBao"] = "Sai thông tin đăng nhập";
                    return View();
                }
                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.MatKhau))
                {
                    if (user.VaiTro == 1)
                    {
                        HttpContext.Session.SetString("UserType", "Customer");
                        HttpContext.Session.SetString("ID", user.MaKh);
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.HoTen), // Replace with your actual data
                            new Claim(MySetting.CLAIM_CUSTOMERID, user.MaKh),

                            new Claim(ClaimTypes.Role, "Customer")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var claimPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(claimPrincipal);

                        if (!string.IsNullOrEmpty(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        return RedirectToAction("CustomerLogin");
                    }
                    else if (user.VaiTro == 0)
                    {
                        HttpContext.Session.SetString("UserType", "Employee");
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.HoTen), // Replace with your actual data
                            new Claim(MySetting.CLAIM_CUSTOMERID, user.MaKh),
                            new Claim("ID", user.MaKh ?? string.Empty),

                            new Claim(ClaimTypes.Role, "Administrator"),
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var claimPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(claimPrincipal);

                        if (!string.IsNullOrEmpty(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        return RedirectToAction("AfterLogin");
                    }
                }
                

                ModelState.AddModelError("", "Invalid username or password.");
            }
            

            return View(model);
        }
    

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var existedUser = _context.KhachHangs.SingleOrDefault(x => x.MaKh == id);
            if (existedUser != null)
            {
                return View(existedUser);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult Edit(Account model, IFormFile FileLogo)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var existedUser = _context.KhachHangs.SingleOrDefault(x => x.MaKh == model.MaKh);
            if (existedUser != null)
            {
                try
                {
                    existedUser.HoTen = model.FullName;
                    existedUser.MaKh = model.UserName;
                    existedUser.DiaChi = model.Address;
                    existedUser.DienThoai = model.PhoneNumber;
                    if (FileLogo != null)
                    {
                        existedUser.Hinh = MyTool.UploadImageToFolder(FileLogo, "Hinh");
                    }
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {

                }
            }
            return NotFound();
        }
        public IActionResult ForgetPass()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgetPass(ForgetPassViewModel model)
        {
            // Kiểm tra xem model có hợp lệ không
            if (ModelState.IsValid)
            {
                var user = _context.KhachHangs.FirstOrDefault(u => u.MaKh == model.username && u.Email == model.email);

                if (user != null)
                {
                    HttpContext.Session.SetString("resetusername", model.username);
                    HttpContext.Session.SetString("resetemail", model.email);

                    return RedirectToAction("ResetPassword", "Accounts");
                }
                else
                {
                    ModelState.AddModelError("", "Username hoặc email không hợp lệ. Vui lòng thử lại.");
                }
            }

            // Trả về lại view cùng với model để giữ lại dữ liệu và hiển thị lỗi
            return View(model);
        }

        [Authorize]
        public IActionResult ResetPassword()
        {
            var user = HttpContext.Session.GetString("resetusername");
            var email = HttpContext.Session.GetString("resetemail");
            var cus = _context.KhachHangs.Where(p => p.MaKh == user && p.Email == email).FirstOrDefault();

            var ResetUser = new ResetPasswordViewModel
            {
                username = user,
                email = email,
                NewPassword = "",
                ConfirmPassword = ""
            };
            return View(ResetUser);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            var maKh = User.FindFirst("ID")?.Value;
            var email = HttpContext.Session.GetString("resetemail");
            var cus = _context.KhachHangs.Where(p => p.MaKh == maKh && p.Email == email).FirstOrDefault();

            cus.MatKhau = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.SaveChanges();
            return View(model);
        }

        [Authorize]
        public IActionResult Profile()
        {
            var userName = User.FindFirst("Username")?.Value;
            var maKh = User.FindFirst("ID")?.Value;
            var user = _context.KhachHangs.FirstOrDefault(u => u.MaKh == maKh);
            return View(user);
        }
        [Authorize]
        public IActionResult ChangePassword()
        {
            var userName = User.FindFirst("Username")?.Value;
            ViewBag.UserName = userName;
            return View();
        }
        [Authorize]
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userName = User.FindFirst("Username")?.Value;
                var maKh = User.FindFirst("ID")?.Value;
                var user = _context.KhachHangs.FirstOrDefault(u => u.MaKh == maKh);
                if (user != null && BCrypt.Net.BCrypt.Verify(model.OldPassword, user.MatKhau))
                {
                    user.MatKhau = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                    _context.SaveChanges();
                }
                return RedirectToAction("Main");
            }
            return View(model);
        }
        public IActionResult QLTK()
        {
            var userName = HttpContext.Session.GetString("Username");
            var maKh = User.FindFirst("ID")?.Value;
            var user = _context.KhachHangs.Where(p => p.MaKh == maKh).FirstOrDefault();
            return View(user);
        }
        [HttpPost]
        public IActionResult QLTK(KhachHang model, IFormFile Hinh)
        {
            try
            {
                var userName = HttpContext.Session.GetString("Username");
                var maKh = User.FindFirst("ID")?.Value;
                var user = _context.KhachHangs.Where(p => p.MaKh == maKh).FirstOrDefault();

                user.HoTen = model.HoTen;
                user.DiaChi = model.DiaChi;
                user.DienThoai = model.DienThoai;
                user.Email = model.Email;
                if (Hinh != null)
                {
                    user.Hinh = MyTool.UploadImageToFolder(Hinh, "Hinh");
                }

                _context.SaveChanges();
                return RedirectToAction("Profile");
            }
            catch (Exception ex) { }
            return View(model);
        }



        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Main");
        }

        [HttpGet]
        public IActionResult Delete(string Id)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var product = _context.KhachHangs.SingleOrDefault(x => x.MaKh == Id);
            if (product != null)
            {
                return View(product);
            }
            return NotFound();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmDelete(string Id)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var user = _context.KhachHangs.SingleOrDefault(x => x.MaKh == Id);
            if (user != null)
            {
                _context.KhachHangs.Remove(user);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return NotFound();
        }
        [Authorize]
        public async Task<IActionResult> Details(string? id)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Employee")
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }
            var product = await _context.KhachHangs.FirstOrDefaultAsync(m => m.MaKh == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
    }
}
