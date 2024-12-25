using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly Hshop2023Context _context;

        public CartViewComponent(Hshop2023Context context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;

            // Lấy danh sách sản phẩm trong giỏ hàng từ cơ sở dữ liệu
            var cartItemsDb = _context.CartItems.Where(ci => ci.MaKh == customerId).ToList();

            // Tạo danh sách CartItem từ CartItemDb
            var cartItems = cartItemsDb.Select(item => new CartItem
            {
                MaHh = item.MaHh,
                Hinh = item.Hinh,
                TenHH = item.TenHH,
                DonGia = item.DonGia,
                SoLuong = item.SoLuong
            }).ToList();

            // Tính tổng số lượng và tổng giá trị
            var totalQuantity = cartItems.Sum(p => p.SoLuong);
            var totalPrice = cartItems.Sum(p => p.ThanhTien);

            return View("CartPanel", new CartModel
            {
                Quantity = totalQuantity,
                Total = totalPrice,
                CartItems = cartItems
            });
        }
    }
}