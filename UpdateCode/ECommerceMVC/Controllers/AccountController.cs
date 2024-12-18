using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

public class AccountController : Controller
{
    private readonly IMapper _mapper;
    private readonly Hshop2023Context db;

    public AccountController(IMapper mapper, Hshop2023Context context)
    {
        _mapper = mapper;
        db = context;
    }

    #region Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(RegisterVM model, IFormFile Hinh)
    {
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
            }
            return View(model);
        }

        try
        {
            var khachHang = _mapper.Map<KhachHang>(model);

            // Tạo key ngẫu nhiên và mã hóa mật khẩu
            khachHang.RandomKey = Util.GenerateRamdomKey();
            khachHang.MatKhau = model.MatKhau.ToMd5Hash(khachHang.RandomKey);

            // Gán giá trị mặc định
            khachHang.HieuLuc = true;
            khachHang.VaiTro = 0;

            // Xử lý hình ảnh
            if (Hinh != null)
            {
                var uploadedImagePath = Util.UploadHinh(Hinh, "KhachHang");
                if (!string.IsNullOrEmpty(uploadedImagePath))
                {
                    khachHang.Hinh = uploadedImagePath;
                }
                else
                {
                    ModelState.AddModelError("", "Lỗi khi tải ảnh.");
                    return View(model);
                }
            }
            else
            {
                khachHang.Hinh = "/Hinh/default-avatar.png";
            }

            // Lưu khách hàng vào database
            db.Add(khachHang);
            db.SaveChanges();

            TempData["Message"] = "Đăng ký thành công!";
            return RedirectToAction("Login"); // Chuyển hướng tới Login
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Register Error: {ex.Message}");
            ModelState.AddModelError("", $"Lỗi hệ thống: {ex.Message}");
        }

        return View(model);
    }

    #endregion

    #region Login
    [HttpGet]
    public IActionResult Login(string? ReturnUrl)
    {
        ViewBag.ReturnUrl = ReturnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVM model, string? ReturnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == model.UserName);
        if (khachHang == null)
        {
            ModelState.AddModelError("loi", "Sai tên đăng nhập hoặc mật khẩu.");
            return View(model);
        }

        if (!khachHang.HieuLuc)
        {
            ModelState.AddModelError("loi", "Tài khoản đã bị khóa.");
            return View(model);
        }

        var hashedPassword = model.Password.ToMd5Hash(khachHang.RandomKey);
        if (khachHang.MatKhau != hashedPassword)
        {
            ModelState.AddModelError("loi", "Sai thông tin đăng nhập.");
            return View(model);
        }

        // Tạo claims
        var claims = new List<Claim> {
        new Claim(ClaimTypes.Email, khachHang.Email),
        new Claim(ClaimTypes.Name, khachHang.HoTen),
        new Claim("CustomerID", khachHang.MaKh),
        new Claim("UserImage", khachHang.Hinh ?? "/Hinh/default-avatar.png"),
        new Claim(ClaimTypes.Role, "Customer")
    };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));

        if (Url.IsLocalUrl(ReturnUrl))
        {
            return Redirect(ReturnUrl);
        }

        return Redirect("/");
    }

    #endregion

    [Authorize]
    public IActionResult Profile()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/");
    }
}
