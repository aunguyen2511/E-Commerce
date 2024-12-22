using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ECommerceMVC.Helpers;
using Microsoft.AspNetCore.Authorization;
using ECommerceMVC.Services;

namespace ECommerceMVC.Controllers
{
	public class CartController : Controller
	{
		private readonly PaypalClient _paypalClient;
		private readonly Hshop2023Context db;
		private readonly IVnPayService _vnPayservice;

		public CartController(Hshop2023Context context, PaypalClient paypalClient, IVnPayService vnPayservice)
		{
			_paypalClient = paypalClient;
			db = context;
			_vnPayservice = vnPayservice;
		}

		public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

		public IActionResult Index()
		{
			return View(Cart);
		}

		public IActionResult AddToCart(int id, int quantity = 1)
		{
			var gioHang = Cart;
			var item = gioHang.SingleOrDefault(p => p.MaHh == id);
			if (item == null)
			{
				var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
				if (hangHoa == null)
				{
					TempData["Message"] = $"Không tìm thấy hàng hóa có mã {id}";
					return Redirect("/404");
				}
				item = new CartItem
				{
					MaHh = hangHoa.MaHh,
					TenHH = hangHoa.TenHh,
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
			if (Cart.Count == 0)
			{
				return Redirect("/");
			}

			ViewBag.PaypalClientdId = _paypalClient.ClientId;
			return View(Cart);
		}

		[Authorize]
		[HttpPost]
		public IActionResult Checkout(CheckoutVM model, string payment = "COD")
		{
			var sessionModel = HttpContext.Session.Get<CheckoutVM>(MySetting.CHECKOUT_MODEL_KEY);
			if(sessionModel != null)
			{
				model = sessionModel;
			}
			var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
			var khachHang = new KhachHang();
			if (!string.IsNullOrEmpty(customerId) && model.GiongKhachHang)
			{
				khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
				if (!string.IsNullOrWhiteSpace(khachHang.DiaChi) && ModelState.ContainsKey(nameof(model.DiaChi)))
				{
					//remove old model state error.
					ModelState.Remove(nameof(model.DiaChi));

					//reassign dia chi from db
					model.DiaChi = khachHang.DiaChi;

					//remove validation error after re-assignment if it had not been previously validated during binding.
					if (ModelState.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
						ModelState.SetModelValue(nameof(model.DiaChi), model.DiaChi, model.DiaChi);
				}
			}

			if (ModelState.IsValid)
			{
				if (payment == "Thanh toán VNPay")
				{

					TempData["Checkout_HoTen"] = model.HoTen;
					TempData["Checkout_DiaChi"] = model.DiaChi;
					TempData["Checkout_DienThoai"] = model.DienThoai;
					TempData["Checkout_GhiChu"] = model.GhiChu;
					TempData["Checkout_GiongKhachHang"] = model.GiongKhachHang;

					// Store the model data in session (serialize it)
					HttpContext.Session.Set<CheckoutVM>(MySetting.CHECKOUT_MODEL_KEY, model);

					var vnPayModel = new VnPaymentRequestModel
					{
						Amount = Cart.Sum(p => p.ThanhTien) * 1000,
						CreatedDate = DateTime.Now,
						Description = $"{model.HoTen} {model.DienThoai}",
						FullName = model.HoTen ?? khachHang.HoTen,
						OrderId = new Random().Next(1000, 100000),
					};
					return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
				}

				//var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
				//var khachHang = new KhachHang();
				//if (model.GiongKhachHang)
				//{
				//	khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
				//}

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

				db.Database.BeginTransaction();
				try
				{

					db.Add(hoadon);
					db.SaveChanges();

					var cthds = new List<ChiTietHd>();
					foreach (var item in Cart)
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
					db.AddRange(cthds);
					db.SaveChanges();
					db.Database.CommitTransaction();

					HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());
					sessionModel = null;
					HttpContext.Session.Set<CheckoutVM>(MySetting.CHECKOUT_MODEL_KEY, sessionModel);
					
					return View("Success");
				}
				catch
				{
					db.Database.RollbackTransaction();
					ViewBag.CheckoutVM = model;
					return View(Cart);
				}
			}
			else
			{
				ViewBag.CheckoutVM = sessionModel ?? model;
				return View(Cart);
				
			}
			
			//return View(Cart);
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
			var model = HttpContext.Session.Get<CheckoutVM>(MySetting.CHECKOUT_MODEL_KEY);

			//Retrieve the values from temp data
			var tempHoTen = TempData["Checkout_HoTen"] as string;
			var tempDiaChi = TempData["Checkout_DiaChi"] as string;
			var tempDienThoai = TempData["Checkout_DienThoai"] as string;
			var tempGhiChu = TempData["Checkout_GhiChu"] as string;
			var tempGiongKhachHang = (bool?)TempData["Checkout_GiongKhachHang"];

			if (tempHoTen == null && tempDiaChi == null && tempDienThoai == null && !tempGiongKhachHang.HasValue) // check mandatory values
			{
				TempData["Message"] = "Error: Checkout Data Missing"; // Handle error if no checkout model in session
				return RedirectToAction("PaymentFail");
			}

			var response = _vnPayservice.PaymentExecute(Request.Query);

			if (response == null || response.VnPayResponseCode != "00")
			{
				TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
				return RedirectToAction("PaymentFail");
			}

			// Lưu đơn hàng vô database

			var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
			var khachHang = new KhachHang();

			if (model.GiongKhachHang)
			{
				khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
				if (!string.IsNullOrWhiteSpace(khachHang.DiaChi) && ModelState.ContainsKey(nameof(model.DiaChi)))
				{
					//remove old model state error.
					ModelState.Remove(nameof(model.DiaChi));

					//reassign dia chi from db
					model.DiaChi = khachHang.DiaChi;

					//remove validation error after re-assignment if it had not been previously validated during binding.
					if (ModelState.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
						ModelState.SetModelValue(nameof(model.DiaChi), model.DiaChi, model.DiaChi);
				}
			}

			if (tempGiongKhachHang == true)
			{
				khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
			}

			var hoadon = new HoaDon
			{
				MaKh = customerId,
				HoTen = tempHoTen ?? khachHang.HoTen, //retrieve from tempdata
				DiaChi = tempDiaChi ?? khachHang.DiaChi ?? response.Address,  //retrieve from tempdata
				DienThoai = tempDienThoai ?? khachHang.DienThoai,   //retrieve from tempdata
				NgayDat = DateTime.Now,
				CachThanhToan = "VNPay",
				CachVanChuyen = "GRAB",
				MaTrangThai = 0,
				GhiChu = tempGhiChu  //retrieve from tempdata
			};

			db.Database.BeginTransaction();
			try
			{

				db.Add(hoadon);
				db.SaveChanges();

				var cthds = new List<ChiTietHd>();
				foreach (var item in Cart)
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
				db.AddRange(cthds);
				db.SaveChanges();
				db.Database.CommitTransaction();

				HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());
				model = null;
				HttpContext.Session.Set<CheckoutVM>(MySetting.CHECKOUT_MODEL_KEY, model);

				TempData["Message"] = $"Thanh toán VNPay thành công";
				return RedirectToAction("PaymentSuccess");
			}
			catch
			{
				db.Database.RollbackTransaction();
				return RedirectToAction("PaymentFail");
			}

			//TempData["Message"] = $"Thanh toán VNPay thành công";
			//return RedirectToAction("PaymentSuccess");
		}

	}
}
