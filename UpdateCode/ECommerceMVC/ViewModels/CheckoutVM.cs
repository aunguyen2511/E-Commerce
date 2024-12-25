namespace ECommerceMVC.ViewModels
{
    public class CheckoutVM
    {
        public bool GiongKhachHang { get; set; }
        public string? HoTen { get; set; }
        public string? DiaChi { get; set; }
        public string? DienThoai { get; set; }
        public string? GhiChu { get; set; }
        public string? CachThanhToan { get; set; } // Thêm thuộc tính này
        public string? CachVanChuyen { get; set; } // Thêm thuộc tính này
    }
}