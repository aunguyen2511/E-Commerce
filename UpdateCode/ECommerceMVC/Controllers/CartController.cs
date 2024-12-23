using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ECommerceMVC.Helpers;
using Microsoft.AspNetCore.Authorization;
using ECommerceMVC.Service;
using System.Linq;
using static System.Net.WebRequestMethods;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly Hshop2023Context db;

        public CartController(Hshop2023Context context, PaypalClient paypalClient)
        {
            _paypalClient = paypalClient;
            db = context;
        }

        public IActionResult Index()
        {
            var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            var cartItems = db.CartItems.Where(ci => ci.MaKh == customerId).ToList();

            var viewModel = cartItems.Select(ci => new CartItem
            {
                MaHh = ci.MaHh,
                Hinh = ci.Hinh,
                TenHH = ci.TenHH,
                DonGia = (double)ci.DonGia,
                SoLuong = ci.SoLuong
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;

            var item = db.CartItems.SingleOrDefault(p => p.MaHh == id && p.MaKh == customerId);
            if (item == null)
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hangHoa == null)
                {
                    return Json(new { success = false, message = $"Không tìm thấy hàng hóa có mã {id}" });
                }
                item = new CartItemDb
                {
                    MaKh = customerId,
                    MaHh = hangHoa.MaHh,
                    TenHH = hangHoa.TenHh,
                    DonGia = hangHoa.DonGia ?? 0,
                    Hinh = hangHoa.Hinh ?? string.Empty,
                    SoLuong = quantity
                };
                db.CartItems.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
                db.CartItems.Update(item);
            }

            db.SaveChanges();

            // Tính tổng số lượng sản phẩm trong giỏ hàng
            var cartItemCount = db.CartItems.Where(c => c.MaKh == customerId).Sum(c => c.SoLuong);

            return Json(new { success = true, message = "Đã thêm vào giỏ hàng", cartItemCount });
        }

        public IActionResult RemoveCart(int id)
        {
            var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            var item = db.CartItems.SingleOrDefault(p => p.MaHh == id && p.MaKh == customerId);
            if (item != null)
            {
                db.CartItems.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult UpdateCart(int id, int quantity)
        {
            var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            var item = db.CartItems.SingleOrDefault(p => p.MaHh == id && p.MaKh == customerId);
            if (item != null)
            {
                item.SoLuong = quantity;
                db.CartItems.Update(item);
                db.SaveChanges();
            }
            return Json(new { success = true });
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;

            // Lấy danh sách sản phẩm trong giỏ hàng từ cơ sở dữ liệu
            var cartItemsDb = db.CartItems.Where(ci => ci.MaKh == customerId).ToList();

            // Chuyển đổi từ CartItemDb sang CartItem
            var cartItems = cartItemsDb.Select(item => new CartItem
            {
                MaHh = item.MaHh,
                Hinh = item.Hinh,
                TenHH = item.TenHH,
                DonGia = (item.DonGia),
                SoLuong = item.SoLuong
            }).ToList();

            ViewBag.PaypalClientId = _paypalClient.ClientId;

            return View(cartItems); // Truyền danh sách cartItems vào view
        }

        [Authorize]
        [HttpPost]
        public IActionResult Checkout(CheckoutVM model)
        {
            HoaDon hoadon = null;

            if (ModelState.IsValid)
            {
                var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
                var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);

                hoadon = new HoaDon
                {
                    MaKh = customerId,
                    HoTen = model.HoTen ?? khachHang.HoTen,
                    DiaChi = model.DiaChi ?? khachHang.DiaChi,
                    DienThoai = model.DienThoai ?? khachHang.DienThoai,
                    NgayDat = DateTime.Now,
                    CachThanhToan = model.CachThanhToan,
                    CachVanChuyen = model.CachVanChuyen,
                    MaTrangThai = 0, // Mới đặt hàng
                    GhiChu = model.GhiChu,
                    ChiTietHds = new List<ChiTietHd>()
                };

                db.Database.BeginTransaction();
                try
                {
                    db.Add(hoadon);
                    db.SaveChanges();

                    var cartItems = db.CartItems.Where(ci => ci.MaKh == customerId).ToList();
                    foreach (var item in cartItems)
                    {
                        hoadon.ChiTietHds.Add(new ChiTietHd
                        {
                            MaHd = hoadon.MaHd,
                            SoLuong = item.SoLuong,
                            DonGia = (double)item.DonGia,
                            MaHh = item.MaHh,
                            GiamGia = 0
                        });
                    }

                    db.AddRange(hoadon.ChiTietHds);
                    db.SaveChanges();

                    // Xóa các sản phẩm trong giỏ hàng
                    db.CartItems.RemoveRange(cartItems);
                    db.SaveChanges();

                    // Cập nhật trạng thái đơn hàng thành 1 (đã thanh toán) nếu là Cash on Delivery
                    if (model.CachThanhToan == "CashOnDelivery")
                    {
                        hoadon.MaTrangThai = 1; // Đã thanh toán
                        db.Update(hoadon);
                        db.SaveChanges();
                    }

                    db.Database.CommitTransaction();
                    return View("Success", hoadon);
                }
                catch (Exception ex)
                {
                    db.Database.RollbackTransaction();
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi xử lý đơn hàng của bạn. Vui lòng thử lại.");
                    Console.WriteLine(ex.Message);
                }
            }

            return View("Success", hoadon);
        }

        [Authorize]
        public async Task<IActionResult> PaymentSuccess()
        {
            // Lấy hóa đơn mới nhất cho khách hàng hiện tại
            var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            var hoadon = db.HoaDons
                .Where(h => h.MaKh == customerId)
                .OrderByDescending(h => h.NgayDat)
                .FirstOrDefault();

            if (hoadon == null)
            {
                return RedirectToAction("Index");
            }

            // Cập nhật trạng thái đơn hàng
            hoadon.MaTrangThai = 1; // Đã thanh toán
            db.Update(hoadon);
            await db.SaveChangesAsync();

            // Trả về view Success với hóa đơn đã được cập nhật
            return View("Success", hoadon);
        }

        #region Paypal payment
        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            var cartItems = db.CartItems.Where(ci => ci.MaKh == customerId).ToList();
            var tongTien = cartItems.Sum(p => p.DonGia * p.SoLuong).ToString();
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
                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }
        #endregion
    }
}