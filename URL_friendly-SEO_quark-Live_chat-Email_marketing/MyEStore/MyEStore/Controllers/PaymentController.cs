using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEStore.Entities;
using MyEStore.Helpers;
using MyEStore.Models;
using MyEStore.Models.Services;

namespace MyEStore.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly MyeStoreContext _context;
        private readonly IVnPayService _vnPayService;

        public PaymentController(PaypalClient paypalClient, MyeStoreContext context, IVnPayService vnPayService)
        {
            _paypalClient = paypalClient;
            _context = context;
            _vnPayService = vnPayService;
        }
        
        #region Payment/Index
        public IActionResult Index()
        {
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(CartItems);
        }
        #endregion

        //public static string CART_KEY = "CART";
        // cái này là copy từ bên CartController, rảnh thì cho nó thành lớp hay gì để dễ reuse 

        // ====> Update: cho nó thành lớp MySetting rồi muahahahahahahahaha!!!

        public List<CartItem> CartItems
        {
            get
            {
                // Trong tài liệu là đoạn comment dưới:
                //var carts = HttpContext.Session.Get<List<CartItem>>("CART");
                //if(carts == null)
                //{
                //    carts = new List<CartItem>();
                //}
                //return carts;

                // Còn cái này là thầy làm cho gọn - hơi khó hiểu...
                return HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
            }

            // Set là làm thêm (ngoài tài liệu)
            set
            {
                HttpContext.Session.Set(MySetting.CART_KEY, value);
            }
        }

        [Authorize]
        [HttpGet]
        #region Payment/PaypalDemo
        public IActionResult PaypalDemo()
        {
            ViewBag.PaypalClientId = _paypalClient.ClientId;

            var maKh = User.FindFirst("ID")?.Value;
            if (string.IsNullOrEmpty(maKh))
            {
                return Unauthorized("Bạn cần đăng nhập để xem giỏ hàng.");
            }

            // Lấy giỏ hàng từ database
            var cart = _context.Carts
                               .Where(c => c.MaKh == maKh)
                               .Include(c => c.MaHhNavigation) // Eager loading
                               .ToList();
            if(cart.Count == 0)
            {
                TempData["ErrorMessage"] = "Bạn cần phải chọn sản phẩm trước khi thanh toán! Hihi :D";
                return Redirect("/");
            }

            // Tạo danh sách CartItem
            var cartItems = cart.Select(c => new CartItem
            {
                MaHh = c.MaHh,
                SoLuong = c.SoLuong,
                TenHh = c.MaHhNavigation != null ? c.MaHhNavigation.TenHh : "Sản phẩm không tồn tại",
                DonGia = c.MaHhNavigation != null ? c.MaHhNavigation.DonGia ?? 0 : 0,
                Hinh = c.MaHhNavigation != null ? c.MaHhNavigation.Hinh : null
            }).ToList();

            return View(cartItems);

            //return View(CartItems); cái này là List Cart Items trong session
        }
        #endregion Payment/PaypalDemo


        [Authorize]
        [HttpPost]
        #region Payment/PaypalDemo
        public IActionResult PaypalDemo(CheckoutVM model)
        {
            ViewBag.PaypalClientId = _paypalClient.ClientId;

            var maKh = User.FindFirst("ID")?.Value;
            if (string.IsNullOrEmpty(maKh))
            {
                return Unauthorized("Bạn cần đăng nhập để xem giỏ hàng.");
            }

            // Lấy giỏ hàng từ database
            var cart = _context.Carts
                               .Where(c => c.MaKh == maKh)
                               .Include(c => c.MaHhNavigation) // Eager loading
                               .ToList();
            if (cart.Count == 0)
            {
                TempData["ErrorMessage"] = "Bạn cần phải chọn sản phẩm trước khi thanh toán! Hihi :D";
                return Redirect("/");
            }

            // Tạo danh sách CartItem
            var cartItems = cart.Select(c => new CartItem
            {
                MaHh = c.MaHh,
                SoLuong = c.SoLuong,
                TenHh = c.MaHhNavigation != null ? c.MaHhNavigation.TenHh : "Sản phẩm không tồn tại",
                DonGia = c.MaHhNavigation != null ? c.MaHhNavigation.DonGia ?? 0 : 0,
                Hinh = c.MaHhNavigation != null ? c.MaHhNavigation.Hinh : null
            }).ToList();


            if (ModelState.IsValid)
            {
                var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;

                var khachHang = new KhachHang();

                if (model.GiongKhachHang)
                {
                    khachHang = _context.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
                }

                var hoadon = new HoaDon
                {
                    MaKh = customerId,
                    HoTen = model.HoTen ?? khachHang.HoTen,
                    DiaChi = model.DiaChi ?? khachHang.DiaChi,
                    DienThoai = model.DienThoai ?? khachHang.DienThoai,
                    NgayDat = DateTime.Now,
                    CachThanhToan = "COD",
                    CachVanChuyen = "GRAB",
                    MaTrangThai = 0,
                    GhiChu = model.GhiChu
                };

                _context.Database.BeginTransaction();

                try 
                {
                    _context.Database.CommitTransaction();
                    _context.Add(hoadon);
                    _context.SaveChanges();

                    var cthds = new List<ChiTietHd>();

                    foreach(var item in cart)
                    {

                        cthds.Add(new ChiTietHd
                        {
                            MaHd = hoadon.MaHd,
                            SoLuong = item.SoLuong,
                            DonGia = item.MaHhNavigation != null ? item.MaHhNavigation.DonGia ?? 0:0,
                            MaHh = item.MaHh,
                            GiamGia = 0
                        });
                    }
                    _context.AddRange(cthds);
                    _context.SaveChanges();
                    HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());

                    return View("Success");
                }

                catch
                {
                    _context.Database.RollbackTransaction();
                }
            }

            return View(cartItems);

            //return View(CartItems); cái này là List Cart Items trong session
        }
        #endregion Payment/PaypalDemo



        //#region Payment/Checkout - COD
        //[Authorize]
        //[HttpPost]
        //public IActionResult Checkout(CheckoutVM model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        //var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
        //        var khachHang = new KhachHang();
        //        //if (model.GiongKhachHang)
        //        {
        //            khachHang = _context.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
        //        }

        //    }

        //    return View(model);
        //}
        //#endregion Payment/Checkout - COD

        #region Payment/PaypalOrder
        [HttpPost]
        public async Task<IActionResult> PaypalOrder(CancellationToken cancellationToken)
        {
            // Tạo đơn hàng (thông tin lấy từ Session???) ???????????????????????????????????????????????? SessionExtensions hử ta?
            var tongTien = CartItems.Sum(p => p.ThanhTien).ToString();
            var donViTienTe = "USD";

            //OrderId là mã tham chiếu duy nhất ???????????????????????????????????????????????????????????
            var orderIdref = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                // a.Create paypal order
                var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, orderIdref);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new
                {
                    ex.GetBaseException().Message
                };
                return BadRequest(error);
            }
        }
        #endregion Payment/PaypalOrder

        #region Payment/PaypalCapture
        public async Task<IActionResult> PaypalCapture(string orderId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderId);
                var reference = response.purchase_units[0].reference_id;

                // Put the logic to save the transaction here ???????????????????
                // Can use the "reference" variable as a transaction key ....?

                // Lưu đơn hàng vô DB

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new
                {
                    ex.GetBaseException().Message
                };
                return BadRequest(error);
            }
        }
        #endregion Payment/PaypalCapture

        #region Payment/Success --------------- cái này để tạm 
        public IActionResult Success()
        {
            return View();
        }
        #endregion Payment/Success --------------- cái này để tạm 

        #region Payment/PaymentCallBack
        [Authorize]
        public IActionResult PaymentCallBack()
        {
            return View();
        }
        #endregion Payment/PaymentCallBack

    }
}
