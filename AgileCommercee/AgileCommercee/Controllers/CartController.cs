using AgileCommercee.Entities;
using AgileCommercee.Models.Helper;
using AgileCommercee.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgileCommercee.Controllers
{
    public class CartController : Controller
    {
        private readonly MyEstoreContext _context;
        public CartController(MyEstoreContext context)
        {
            _context = context;
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
                return HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
            }

            // Set là làm thêm (ngoài tài liệu)
            set
            {
                HttpContext.Session.Set(MySetting.CART_KEY, value);
            }
        }

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
            var cart = _context.Carts
                               .Where(c => c.MaKh == maKh)
                               .Include(c => c.MaHhNavigation) // Eager loading
                               .ToList();

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
        }
        #endregion Cart - Index

        #region Cart - AddToCart
        public IActionResult AddToCart(int id, int qty = 1)
        {
            // Thầy làm là "cart" thay vì "gioHang"
            var gioHang = CartItems;

            // kiểm tra id (MaHh) truyền qua đã nằm trong giỏ hàng hay chưa
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item != null) // đã có
            {
                item.SoLuong += qty;
            }
            else
            {
                var hangHoa = _context.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hangHoa == null) // id không có trong DB
                {
                    //return RedirectToAction("Index", "HangHoas");
                    return NotFound();
                }
                item = new CartItem
                {
                    MaHh = id,
                    SoLuong = qty,
                    TenHh = hangHoa.TenHh,
                    Hinh = hangHoa.Hinh,
                    //DonGia = hangHoa.DonGia.Value
                    DonGia = hangHoa.DonGia ?? 0
                };

                // thêm vào giỏ hàng
                gioHang.Add(item);
            }

            // cập nhật session

            // chỗ này thay vì xài Session.Set thì xài "CartItems = gioHang;" được
            //HttpContext.Session.Set("CART", gioHang);
            CartItems = gioHang;
            var maKh = User.FindFirst("ID")?.Value;
            if (!string.IsNullOrEmpty(maKh))
            {
                var existingCart = _context.Carts.SingleOrDefault(c => c.MaKh == maKh && c.MaHh == id);
                if (existingCart != null)
                {
                    existingCart.SoLuong = gioHang.Single(p => p.MaHh == id).SoLuong;
                }
                else
                {
                    var newCart = new Cart
                    {
                        MaKh = maKh,
                        MaHh = id,
                        SoLuong = qty,
                        NgayThem = DateTime.Now
                    };
                    _context.Carts.Add(newCart);
                }
                _context.SaveChanges();
            }
            return RedirectToAction("Index"); // để hiển thị giỏ hàng

        }
        #endregion Cart - Add To Cart

        //#region Cart - RemoveCartItem - Session
        //public IActionResult RemoveCartItem(int id)
        //{
        //    var gioHang = CartItems;
        //    var item = gioHang.SingleOrDefault(p => p.MaHh == id);
        //    if(item != null) // kiểm tra nếu giỏ hàng có chứa sản phẩm ở trỏng
        //    {
        //        gioHang.Remove(item);
        //        CartItems = gioHang;
        //        //HttpContext.Session.Set("CART", gioHang);
        //    }
        //    return RedirectToAction("Index");
        //}
        //#endregion Cart - RemoveCartItem

        [Authorize]
        #region Cart - RemoveCartItem
        public IActionResult RemoveCartItem(int id)
        {
            var maKh = User.FindFirst("ID")?.Value;
            if (string.IsNullOrEmpty(maKh))
            {
                return Unauthorized("Cần đăng nhập để xem giỏ hàng");
            }
            var cartItems = _context.Carts.SingleOrDefault(c => c.MaKh == maKh && c.MaHh == id);
            if (cartItems != null)
            {
                // Xoá sản phẩm khỏi giỏ hàng
                _context.Carts.Remove(cartItems);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        #endregion Cart - RemoveCartItem

        //#region Cart - ClearCart -- Session
        //public IActionResult ClearCart()
        //{
        //    CartItems = new List<CartItem>();
        //    return RedirectToAction("Index");
        //}
        //#endregion Cart - ClearCart

        /* phần trên là lưu Cart trong Session, còn Database thì phải xác thực, liên kết với tài khoản người dùng
         * về nhà coi làm thêm cái update (giỏ hàng hay gì??)
         */

        [Authorize]
        #region Cart - ClearCart
        public IActionResult ClearCart()
        {
            var maKh = User.FindFirst("ID")?.Value;
            if (string.IsNullOrEmpty(maKh))
            {
                return Unauthorized("Cần đăng nhập để xem giỏ hàng");
            }

            // Lấy tất cả sản phẩm trong giỏ hàng của khách hàng từ DB
            var cartItems = _context.Carts.Where(c => c.MaKh == maKh).ToList();

            // Xoá tất cả sản phẩm vừa lấy trong cartItems
            _context.Carts.RemoveRange(cartItems);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        #endregion Cart - ClearCart


        //#region Cart - UpdateQuantity - Session
        //[HttpPost]
        //public IActionResult UpdateQuantity(int maHh, int soLuong)
        //{
        //    var gioHang = CartItems;
        //    var item = gioHang.SingleOrDefault(p => p.MaHh == maHh);

        //    if (item != null)
        //    {
        //        // Cập nhật số lượng
        //        item.SoLuong = soLuong > 0 ? soLuong : 1; // Đảm bảo số lượng không âm
        //        CartItems = gioHang; // Lưu lại giỏ hàng vào session
        //    }

        //    return Ok();
        //}
        //#endregion Cart - UpdateQuantity

        #region Cart - UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int maHh, int soLuong)
        {
            var maKh = User.FindFirst("ID")?.Value;
            if (string.IsNullOrEmpty(maKh))
            {
                return Unauthorized("Cần đăng nhập");
            }
            // Lấy sản phẩm từ giỏ hàng trong DB
            var cartItems = _context.Carts.SingleOrDefault(p => p.MaKh == maKh && p.MaHh == maHh);
            if (cartItems != null)
            {
                cartItems.SoLuong = soLuong > 0 ? soLuong : 1;
                _context.SaveChanges();
            }
            return Ok();
        }
        #endregion Cart - UpdateQuantity

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
    }
}
