using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEStore.Entities;
using MyEStore.Models;

namespace MyEStore.Controllers
{
    public class CartController : Controller
    {
        private readonly MyeStoreContext _context;
        public CartController(MyeStoreContext context)
        {
            _context = context;
        }

        // Reuse tên - muốn đổi thì đổi trong ngoặc thôi - do là sai tên thì nó quánh lộn
        public static string CART_KEY = "CART";

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
                return HttpContext.Session.Get<List<CartItem>> (CART_KEY) ?? new List<CartItem> ();
            }

            // Set là làm thêm (ngoài tài liệu)
            set
            {
                HttpContext.Session.Set(CART_KEY, value);
            }
        }

        #region Cart - Index
        public IActionResult Index()
        {
            return View(CartItems);
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
            return RedirectToAction("Index"); // để hiển thị giỏ hàng

        }
        #endregion Cart - Add To Cart

        #region Cart - RemoveCartItem
        public IActionResult RemoveCartItem(int id)
        {
            var gioHang = CartItems;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if(item != null) // kiểm tra nếu giỏ hàng có chứa sản phẩm ở trỏng
            {
                gioHang.Remove(item);
                CartItems = gioHang;
                //HttpContext.Session.Set("CART", gioHang);
            }
            return RedirectToAction("Index");
        }
        #endregion Cart - RemoveCartItem

        #region Cart - ClearCart
        public IActionResult ClearCart()
        {
            CartItems = new List<CartItem>();
            return RedirectToAction("Index");
        }
        #endregion Cart - ClearCart

        /* phần trên là lưu Cart trong Session, còn Database thì phải xác thực, liên kết với tài khoản người dùng
         * về nhà coi làm thêm cái update (giỏ hàng hay gì??)
         */

        #region Cart - UpdateQuantity
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
    }
}
