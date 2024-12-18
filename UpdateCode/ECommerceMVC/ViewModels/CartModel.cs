using System.Collections.Generic;

namespace ECommerceMVC.ViewModels
{
    public class CartModel
    {
        public int Quantity { get; set; }
        public double Total { get; set; }
        public List<CartItem> CartItems { get; set; } // Danh sách sản phẩm trong giỏ hàng
    }
}