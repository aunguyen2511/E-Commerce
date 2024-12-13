using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEStore.Entities;
using MyEStore.Helpers;
using MyEStore.Models;
using MyEStore.Models.Services;
using System;

namespace MyEStore.Controllers
{
    public class CartController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly MyeStoreContext _context;
        private readonly IVnPayService _vnPayService;
        public CartController(PaypalClient paypalClient, MyeStoreContext context, IVnPayService vnPayService)
        {
            _paypalClient = paypalClient;
            _context = context;
            _vnPayService = vnPayService;
        }

        // Reuse tên - muốn đổi thì đổi trong ngoặc thôi - do là sai tên thì nó quánh lộn

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
                return HttpContext.Session.Get<List<CartItem>> (MySetting.CART_KEY) ?? new List<CartItem> ();
            }

            // Set là làm thêm (ngoài tài liệu)
            set
            {
                HttpContext.Session.Set(MySetting.CART_KEY, value);
            }
        }

        public IActionResult Index()
        {
            return View(CartItems);
        }

        //[Authorize]
        //#region Cart - Index - DB
        //public IActionResult Index()
        //{
        //    var maKh = User.FindFirst("ID")?.Value;
        //    if (string.IsNullOrEmpty(maKh)) 
        //    {
        //        return Unauthorized("Bạn cần đăng nhập để xem giỏ hàng.");
        //    }

        //    // Lấy giỏ hàng từ database
        //    var cart = _context.Carts
        //                       .Where(c => c.MaKh == maKh)
        //                       .Include(c => c.MaHhNavigation) // Eager loading
        //                       .ToList();

        //    // Tạo danh sách CartItem
        //    var cartItems = cart.Select(c => new CartItem
        //    {
        //        MaHh = c.MaHh,
        //        SoLuong = c.SoLuong,
        //        TenHh = c.MaHhNavigation != null ? c.MaHhNavigation.TenHh : "Sản phẩm không tồn tại",
        //        DonGia = c.MaHhNavigation != null ? c.MaHhNavigation.DonGia ?? 0 : 0,
        //        Hinh = c.MaHhNavigation != null ? c.MaHhNavigation.Hinh : null
        //    }).ToList();

        //    return View(cartItems);
        //}
        //#endregion Cart - Index - DB

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var gioHang = CartItems;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                var hangHoa = _context.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hangHoa == null)
                {
                    TempData["Message"] = $"Không tìm thấy hàng hóa có mã {id}";
                    return Redirect("/404");
                }
                item = new CartItem
                {
                    MaHh = hangHoa.MaHh,
                    TenHh = hangHoa.TenHh,
                    DonGia = hangHoa.DonGia ?? 0,
                    Hinh = hangHoa.Hinh ?? string.Empty,
                    SoLuong = quantity
                };
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);

            return RedirectToAction("Index");
        }

        //#region Cart - AddToCart - DB
        //public IActionResult AddToCart(int id, int qty = 1)
        //{
        //    // Thầy làm là "cart" thay vì "gioHang"
        //    var gioHang = CartItems;

        //    // kiểm tra id (MaHh) truyền qua đã nằm trong giỏ hàng hay chưa
        //    var item = gioHang.SingleOrDefault(p => p.MaHh == id);
        //    if (item != null) // đã có
        //    {
        //        item.SoLuong += qty;
        //    }
        //    else
        //    {
        //        var hangHoa = _context.HangHoas.SingleOrDefault(p => p.MaHh == id);
        //        if (hangHoa == null) // id không có trong DB
        //        {
        //            //return RedirectToAction("Index", "HangHoas");
        //            TempData["Message"] = $"Không tìm thấy hàng hóa có mã {id}";
        //            return Redirect("/404");
        //        }
        //        item = new CartItem
        //        {
        //            MaHh = hangHoa.MaHh,
        //            SoLuong = qty,
        //            TenHh = hangHoa.TenHh,
        //            Hinh = hangHoa.Hinh ?? string.Empty,
        //            //DonGia = hangHoa.DonGia.Value
        //            DonGia = hangHoa.DonGia ?? 0
        //        };

        //        // thêm vào giỏ hàng
        //        gioHang.Add(item);
        //    }

        //    // cập nhật session

        //    // chỗ này thay vì xài Session.Set thì xài "CartItems = gioHang;" được
        //    //HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
        //    CartItems = gioHang;
        //    // nếu đã đăng nhập thì lưu vô DB
        //    // Lấy giỏ hàng từ database
        //    var cart = _context.Carts
        //                       .Where(c => c.MaKh == maKh)
        //                       .Include(c => c.MaHhNavigation) // Eager loading
        //                       .ToList();
        //    var existingCart = _context.Carts.Where();

        //    if (existingCart != null)
        //    {
        //        existingCart.SoLuong += item.SoLuong; // chỗ này cập nhật số lượng nếu có
        //        _context.Carts.Update(existingCart);
        //    }
        //    else
        //    {
        //        var newCart = new Cart
        //        {
        //            MaKh = maKh,
        //            MaHh = id,
        //            SoLuong = qty,
        //            NgayThem = DateTime.Now
        //        };
        //        _context.Carts.Add(newCart);
        //    }
        //    _context.SaveChanges();
        //    return RedirectToAction("Index"); // để hiển thị giỏ hàng

        //}
        //#endregion Cart - AddToCart - DB



        #region Cart - RemoveCartItem - Session
        public IActionResult RemoveCartItem(int id)
        {
            var gioHang = CartItems;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item != null) // kiểm tra nếu giỏ hàng có chứa sản phẩm ở trỏng
            {
                gioHang.Remove(item);
                CartItems = gioHang;
                //HttpContext.Session.Set("CART", gioHang);
            }
            return RedirectToAction("Index");
        }
        #endregion Cart - RemoveCartItem

        //[Authorize]
        //#region Cart - RemoveCartItem
        //public IActionResult RemoveCartItem(int id)
        //{
        //    var maKh = User.FindFirst("ID")?.Value;
        //    if (string.IsNullOrEmpty(maKh))
        //    {
        //        return Unauthorized("Cần đăng nhập để xem giỏ hàng");
        //    }
        //    var cartItems = _context.Carts.SingleOrDefault(c => c.MaKh == maKh && c.MaHh == id);
        //    if (cartItems != null)
        //    {
        //        // Xoá sản phẩm khỏi giỏ hàng
        //        _context.Carts.Remove(cartItems);
        //        _context.SaveChanges();
        //    }
        //    return RedirectToAction("Index");
        //}
        //#endregion Cart - RemoveCartItem

        #region cart - clearcart -- session
        public IActionResult ClearCart()
        {
            CartItems = new List<CartItem>();
            return RedirectToAction("Index");
        }
        #endregion cart - clearcart

        /* phần trên là lưu cart trong session, còn database thì phải xác thực, liên kết với tài khoản người dùng
         * về nhà coi làm thêm cái update (giỏ hàng hay gì??)
         */

        //[Authorize]
        //#region Cart - ClearCart
        //public IActionResult ClearCart()
        //{
        //    var maKh = User.FindFirst("ID")?.Value;
        //    if (string.IsNullOrEmpty(maKh)) 
        //    {
        //        return Unauthorized("Cần đăng nhập để xem giỏ hàng");
        //    }

        //    // Lấy tất cả sản phẩm trong giỏ hàng của khách hàng từ DB
        //    var cartItems = _context.Carts.Where(c => c.MaKh == maKh).ToList();

        //    // Xoá tất cả sản phẩm vừa lấy trong cartItems
        //    _context.Carts.RemoveRange(cartItems);
        //    _context.SaveChanges();

        //    return RedirectToAction("Index");
        //}
        //#endregion Cart - ClearCart


        #region Cart - UpdateQuantity - Session
        [HttpPost]
        public IActionResult UpdateQuantity(int maHh, int soLuong)
        {
            var gioHang = CartItems;
            var item = gioHang.SingleOrDefault(p => p.MaHh == maHh);

            if (item != null)
            {
                // Cập nhật số lượng
                item.SoLuong = soLuong > 0 ? soLuong : 1; // Đảm bảo số lượng không âm
                CartItems = gioHang; // Lưu lại giỏ hàng vào session
            }

            return Ok();
        }
        #endregion Cart - UpdateQuantity

        //#region Cart - UpdateQuantity
        //[HttpPost]
        //public IActionResult UpdateQuantity(int maHh, int soLuong)
        //{
        //    var maKh = User.FindFirst("ID")?.Value;
        //    if (string.IsNullOrEmpty(maKh))
        //    {
        //        return Unauthorized("Cần đăng nhập");
        //    }
        //    // Lấy sản phẩm từ giỏ hàng trong DB
        //    var cartItems = _context.Carts.SingleOrDefault(p => p.MaKh == maKh && p.MaHh == maHh);
        //    if(cartItems != null)
        //    {
        //        cartItems.SoLuong = soLuong > 0 ? soLuong : 1;
        //        _context.SaveChanges();
        //    }
        //    return Ok();
        //}
        //#endregion Cart - UpdateQuantity

        #region Cart - GetCart
        [Authorize]
        public IActionResult GetCart()
        {
            var maKh = User.FindFirst("ID")?.Value;
            if (string.IsNullOrEmpty(maKh))
            {
                return Unauthorized("Bạn cần đăng nhập để xem giỏ hàng.");
            }

            // Lấy giỏ hàng từ DB
            var cart = _context.Carts
                                    .Where(c => c.MaKh == maKh)
                                    .Include(c => c.MaHhNavigation) // Eager loading?
                                    .ToList();

            // Tạo danh sách cartItems
            var cartItems = cart.Select(c => new CartItem
            {
                MaHh = c.MaHh,
                SoLuong = c.SoLuong,
                TenHh = c.MaHhNavigation != null ? c.MaHhNavigation.TenHh : "Sản phẩm không tồn tại",
                DonGia = c.MaHhNavigation != null ? c.MaHhNavigation.DonGia ?? 0 : 0,
                Hinh = c.MaHhNavigation != null ? c.MaHhNavigation.TenHh : null,
            }).ToList();
            return View(cartItems);
        }
        #endregion Cart - GetCart

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            if (CartItems.Count == 0)
            {
                return Redirect("/");
            }

            ViewBag.PaypalClientdId = _paypalClient.ClientId;
            return View(CartItems);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Checkout(CheckoutVM model, string payment = "COD")
        {
            if (ModelState.IsValid)
            {
                if (payment == "Thanh toán VNPay")
                {
                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount = CartItems.Sum(p => p.ThanhTien),
                        CreatedDate = DateTime.Now,
                        Description = $"{model.HoTen} {model.DienThoai}",
                        FullName = model.HoTen,
                        OrderId = new Random().Next(1000, 100000)
                    };
                    return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
                }

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

                    _context.Add(hoadon);
                    _context.SaveChanges();

                    var cthds = new List<ChiTietHd>();
                    foreach (var item in CartItems)
                    {
                        cthds.Add(new ChiTietHd
                        {
                            MaHd = hoadon.MaHd,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            MaHh = item.MaHh,
                            GiamGia = 0
                        });
                    }
                    _context.AddRange(cthds);
                    _context.SaveChanges();
                    _context.Database.CommitTransaction();

                    HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());

                    return View("Success");
                }
                catch
                {
                    _context.Database.RollbackTransaction();
                }
            }

            return View(CartItems);
        }

        [Authorize]
        public IActionResult PaymentSuccess()
        {
            return View("Success");
        }

        #region Paypal payment
        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            // Thông tin đơn hàng gửi qua Paypal
            var tongTien = CartItems.Sum(p => p.ThanhTien).ToString();
            var donViTienTe = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [Authorize]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);

                // Lưu database đơn hàng của mình

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        #endregion

        [Authorize]
        public IActionResult PaymentFail()
        {
            return View();
        }

        [Authorize]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }


            // Lưu đơn hàng vô database

            TempData["Message"] = $"Thanh toán VNPay thành công";
            return RedirectToAction("PaymentSuccess");
        }
    }
}
