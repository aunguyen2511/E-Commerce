namespace ECommerceMVC.Data
{
    public class CartItemDb
    {
        public int Id { get; set; }
        public string MaKh { get; set; } // Mã khách hàng
        public int MaHh { get; set; } // Mã hàng hóa
        public string TenHH { get; set; }
        public double DonGia { get; set; }
        public string Hinh { get; set; }
        public int SoLuong { get; set; }
        public DateTime NgàyTạo { get; set; } = DateTime.Now;
    }
}