using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AgileCommercee.Entities;
using AgileCommercee.Models.Helper;
using AgileCommercee.Models.Service;
using AgileCommercee.Models;
using AgileCommercee;
using Microsoft.EntityFrameworkCore;

namespace AgileCommercee.Controllers
{
    [Authorize]
    public class CartController : Controller
	{
		private readonly PaypalClient _paypalClient;
		private readonly MyEstoreContext db;
		private readonly IVnPayService _vnPayservice;

		public CartController(MyEstoreContext context, PaypalClient paypalClient, IVnPayService vnPayservice)
		{
			_paypalClient = paypalClient;
			db = context;
			_vnPayservice = vnPayservice;
		}

		public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

		[Authorize]
		#region Cart - Index
		public IActionResult Index()
		{
			var maKh = User.FindFirst("ID")?.Value;
			if (string.IsNullOrEmpty(maKh))
			{
				return Unauthorized("Bạn cần đăng nhập để xem giỏ hàng.");
			}

			// Lấy giỏ hàng từ database
			var carts = db.Carts
								.Where(c => c.MaKh == maKh)
								.Include(c => c.MaHhNavigation) // Eager loading
								.ToList();

			// Tạo danh sách CartItem
			var cartItems = carts.Select(c => new CartItem
			{
				MaHh = c.MaHh,
				SoLuong = c.SoLuong,
				TenHh = c.MaHhNavigation != null ? c.MaHhNavigation.TenHh : "Sản phẩm không tồn tại",
				DonGia = c.MaHhNavigation != null ? c.MaHhNavigation.DonGia ?? 0 : 0,
				Hinh = c.MaHhNavigation != null ? c.MaHhNavigation.Hinh : null
			}).ToList();

			return View(cartItems);
		}
		#endregion Cart - Index


		public IActionResult AddToCart(int id, int quantity = 1)
		{
			var maKh = HttpContext.User.Claims.SingleOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;

			if (string.IsNullOrEmpty(maKh))
			{
				TempData["Message"] = "Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng.";
				return RedirectToAction("Login", "Account");
			}

			var cartItem = db.Carts.SingleOrDefault(c => c.MaHh == id && c.MaKh == maKh);

			if (cartItem == null)
			{
				var hangHoa = db.HangHoas.SingleOrDefault(hh => hh.MaHh == id);
				if (hangHoa == null)
				{
					TempData["Message"] = $"Không tìm thấy hàng hóa có mã {id}.";
					return RedirectToAction("Index");
				}

				cartItem = new Cart
				{
					MaKh = maKh,
					MaHh = id,
					SoLuong = quantity,
					NgayThem = DateTime.Now
				};

				db.Carts.Add(cartItem);
			}
			else
			{
				cartItem.SoLuong += quantity;
				cartItem.NgayThem = DateTime.Now;
				db.Carts.Update(cartItem);
			}

			db.SaveChanges();

			TempData["Message"] = "Sản phẩm đã được thêm vào giỏ hàng.";
			return RedirectToAction("Index");
		}



		public IActionResult RemoveCart(int id)
		{
			var gioHang = Cart;
			var item = gioHang.SingleOrDefault(p => p.MaHh == id);
			if (item != null)
			{
				gioHang.Remove(item);
				HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
			}
			return RedirectToAction("Index");
		}

		[Authorize]
		[HttpGet]
		public IActionResult Checkout()
		{
			ViewBag.PaypalClientId = _paypalClient.ClientId;
			var maKh = HttpContext.User.Claims.SingleOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;

			if (string.IsNullOrEmpty(maKh))
			{
				TempData["Message"] = "Bạn cần đăng nhập để thanh toán.";
				return RedirectToAction("Login", "Account");
			}

			var gioHang = db.Carts
				.Where(c => c.MaKh == maKh)
				.Select(c => new CartItem
				{
					MaHh = c.MaHh,
					TenHh = c.MaHhNavigation.TenHh,
					DonGia = c.MaHhNavigation.DonGia ?? 0,
					SoLuong = c.SoLuong,
					Hinh = c.MaHhNavigation.Hinh ?? string.Empty
				})
				.ToList();

			if (!gioHang.Any())
			{
				TempData["Message"] = "Giỏ hàng của bạn đang trống.";
				return RedirectToAction("Index", "Home");
			}

			HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
			ViewBag.PaypalClientId = _paypalClient.ClientId;

			return View(gioHang);
		}


		[Authorize]
		[HttpPost]
		public IActionResult Checkout(CheckoutVM model, string payment = "COD")
		{
			if (ModelState.IsValid)
			{

				ViewBag.PaypalClientId = _paypalClient.ClientId;
				if (payment == "Thanh toán VNPay")
				{
					var vnPayModel = new VnPaymentRequestModel
					{
						Amount = Cart.Sum(p => p.ThanhTien* 25430),
						CreatedDate = DateTime.Now,
						Description = $"{model.HoTen} {model.DienThoai}",
						FullName = model.HoTen,
						OrderId = new Random().Next(1000, 100000)
					};
					return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
				}
				var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;

				if (string.IsNullOrEmpty(customerId))
				{
					TempData["Message"] = "Bạn cần đăng nhập để thanh toán.";
					return RedirectToAction("Login", "Account");
				}

				var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);

				var hoadon = new HoaDon
				{
					MaKh = customerId,
					HoTen = model.HoTen ?? khachHang?.HoTen,
					DiaChi = model.DiaChi ?? khachHang?.DiaChi,
					Dienthoai = model.DienThoai ?? khachHang?.DienThoai,
					NgayDat = DateTime.Now,
					CachThanhToan = payment,
					CachVanChuyen = "GRAB",
					MaTrangThai = 0,
					GhiChu = model.GhiChu
				};

				db.Database.BeginTransaction();
				try
				{
					db.HoaDons.Add(hoadon);
					db.SaveChanges();
					var cthds = Cart.Select(item => new ChiTietHd
					{
						MaHd = hoadon.MaHd,
						SoLuong = item.SoLuong,
						DonGia = item.DonGia,
						MaHh = item.MaHh,
						GiamGia = 0
					}).ToList();
                    
                    db.ChiTietHds.AddRange(cthds);
					db.SaveChanges();
					var cartsToRemove = db.Carts.Where(c => c.MaKh == customerId).ToList();
					db.Carts.RemoveRange(cartsToRemove);
					db.SaveChanges();
					db.Database.CommitTransaction();

					HttpContext.Session.Set(MySetting.CART_KEY, new List<CartItem>());
                    ViewBag.HoaDon = hoadon;
                    ViewBag.CartItems = cthds;
                    return View("Success");
				}
				catch (Exception)
				{
					db.Database.RollbackTransaction();
					TempData["Message"] = "Đã xảy ra lỗi trong quá trình thanh toán. Vui lòng thử lại.";
				}
			}

			return View(Cart);
		}


        public async Task<IActionResult> Success(int maHd)
        {
            var hoadon = db.HoaDons.FirstOrDefault(h => h.MaHd == maHd);
            var cthds = db.ChiTietHds.Where(c => c.MaHd == maHd).ToList();
            ViewBag.HoaDon = hoadon;
            ViewBag.CartItems = cthds;
            return View("Success");
        }

        [HttpGet]
        public async Task<IActionResult> SuccessPayPal()
        {
			return View();
		}

        [HttpGet]
        public async Task<IActionResult> SuccessVnPay()
        {
            return View();
        }

        #region Paypal payment
        [Authorize]
		[HttpPost("/Cart/create-paypal-order")]
		public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
		{
			// Thông tin đơn hàng gửi qua Paypal
			var tongTien = Cart.Sum(p => p.ThanhTien).ToString();
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
		public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken, CheckoutVM model)
		{
			try
			{
				var response = await _paypalClient.CaptureOrder(orderID);
				if (response.status == "COMPLETED")
				{
					var maKh = HttpContext.User.Claims.SingleOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;

					if (string.IsNullOrEmpty(maKh))
					{
						return Unauthorized("Bạn cần đăng nhập để hoàn tất thanh toán.");
					}
					var carts = db.Carts.Where(c => c.MaKh == maKh).Include(c => c.MaHhNavigation).ToList();
					if (!carts.Any())
					{
						return BadRequest("Giỏ hàng trống.");
					}
					var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == maKh);
					var hoadon = new HoaDon
					{
						MaKh = maKh,
						HoTen = model.HoTen ?? khachHang?.HoTen,
						DiaChi = model.DiaChi ?? khachHang?.DiaChi,
						Dienthoai = model.DienThoai ?? khachHang?.DienThoai,
						NgayDat = DateTime.Now,
						CachThanhToan = "Paypal",
						CachVanChuyen = "GRAB",
						MaTrangThai = 0,
						GhiChu = model.GhiChu
					};

					db.HoaDons.Add(hoadon);
					db.SaveChanges();
					var cthds = carts.Select(item => new ChiTietHd
					{
						MaHd = hoadon.MaHd,
						MaHh = item.MaHh,
						SoLuong = item.SoLuong,
						DonGia = item.MaHhNavigation.DonGia ?? 0,
						GiamGia = 0
					}).ToList();
                    
                    db.ChiTietHds.AddRange(cthds);
					db.SaveChanges();
					db.Carts.RemoveRange(carts);
					db.SaveChanges();
					int maHd = hoadon.MaHd;
					HttpContext.Session.Set(MySetting.CART_KEY, new List<CartItem>());
                    ViewBag.HoaDon = hoadon;
                    ViewBag.CartItems = cthds;
					return Ok(response);
				}
				else
				{
					return BadRequest("Thanh toán không thành công.");
				}
			}
			catch (Exception ex)
			{
				var error = new { ex.GetBaseException().Message };
				return BadRequest(error);
			}
		}



		#endregion Paypal Payment

		[Authorize]
		public IActionResult PaymentFail()
		{
			return View();
		}

		[Authorize]
		public IActionResult PaymentCallBack()
		{
			var response = _vnPayservice.PaymentExecute(Request.Query);

			if (response == null || response.VnPayResponseCode != "00")
			{
				TempData["Message"] = $"Lỗi thanh toán VNPay: {response?.VnPayResponseCode ?? "Không xác định"}";
				return RedirectToAction("PaymentFail");
			}

			// Nếu thanh toán thành công, lưu đơn hàng vào database
			var maKh = HttpContext.User.Claims.SingleOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;

			if (string.IsNullOrEmpty(maKh))
			{
				TempData["Message"] = "Bạn cần đăng nhập để hoàn tất thanh toán.";
				return RedirectToAction("Login", "Account");
			}

			var carts = db.Carts.Where(c => c.MaKh == maKh).Include(c => c.MaHhNavigation).ToList();
			if (!carts.Any())
			{
				TempData["Message"] = "Giỏ hàng của bạn đang trống.";
				return RedirectToAction("Index", "Home");
			}

			var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == maKh);
			var hoadon = new HoaDon
			{
				MaKh = maKh,
				HoTen = khachHang?.HoTen,
				DiaChi = khachHang?.DiaChi,
				Dienthoai = khachHang?.DienThoai,
				NgayDat = DateTime.Now,
				CachThanhToan = "VNPay",
				CachVanChuyen = "GRAB",
				MaTrangThai = 0
			};

			db.HoaDons.Add(hoadon);
			db.SaveChanges();

			var cthds = carts.Select(item => new ChiTietHd
			{
				MaHd = hoadon.MaHd,
				MaHh = item.MaHh,
				SoLuong = item.SoLuong,
				DonGia = item.MaHhNavigation.DonGia ?? 0,
				GiamGia = 0
			}).ToList();

			db.ChiTietHds.AddRange(cthds);
			db.Carts.RemoveRange(carts);
			db.SaveChanges();

			HttpContext.Session.Set(MySetting.CART_KEY, new List<CartItem>());

			TempData["Message"] = $"Thanh toán VNPay thành công cho đơn hàng";
			return RedirectToAction("SuccessVnPay");
		}



		#region Cart - UpdateQuantity
		[HttpPost]
		public IActionResult UpdateQuantity(int maHh, int soLuong)
		{
			var maKh = HttpContext.User.Claims.SingleOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;

			if (string.IsNullOrEmpty(maKh))
			{
				TempData["Message"] = "Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng.";
				return RedirectToAction("Login", "Account");
			}
			if (string.IsNullOrEmpty(maKh))
			{
				return Unauthorized("Bạn cần đăng nhập để cập nhật giỏ hàng.");
			}

			if (soLuong <= 0)
			{
				return BadRequest("Số lượng phải lớn hơn 0.");
			}

			var cartItem = db.Carts.SingleOrDefault(c => c.MaKh == maKh && c.MaHh == maHh);

			if (cartItem != null)
			{
				cartItem.SoLuong = soLuong;
				db.SaveChanges();
			}
			else
			{
				return NotFound("Sản phẩm không tồn tại trong giỏ hàng.");
			}

			return Ok();
		}
        #endregion Cart - UpdateQuantity


        #region purchase History
        [Authorize]
        public IActionResult PurchaseHistory()
        {
            var maKh = User.FindFirst("ID")?.Value;

            if (string.IsNullOrEmpty(maKh))
            {
                TempData["Message"] = "Bạn cần đăng nhập để xem lịch sử mua hàng.";
                return RedirectToAction("Login", "Account");
            }
            var hoaDons = db.HoaDons
                            .Where(hd => hd.MaKh == maKh)
                            .Select(hd => new
                            {
                                hd.MaHd,
                                hd.NgayDat,
                                hd.CachThanhToan,
                                hd.CachVanChuyen,
                                hd.GhiChu,
                                hd.MaTrangThai,
                                TongTien = hd.ChiTietHds.Sum(ct => ct.DonGia * ct.SoLuong)
                            })
                            .ToList();

            return View(hoaDons);
        }

        #endregion purchase

        #region purchase details
        public IActionResult PurchaseDetail(int maHd)
        {
            var details = db.ChiTietHds
                .Where(ct => ct.MaHd == maHd)
                .Select(ct => new PurchaseDetailVM
                {
                    TenSP = ct.MaHhNavigation.TenHh,
                    Hinh = ct.MaHhNavigation.Hinh,
                    DonGia = ct.DonGia,
                    SoLuong = ct.SoLuong,
                    GiamGia = ct.GiamGia,
                    ThanhTien = (ct.DonGia - ct.DonGia * ct.GiamGia / 100) * ct.SoLuong
                }).ToList();

            ViewBag.MaHD = maHd;
            return View(details);
        }


        #endregion purchase details

    }

}
