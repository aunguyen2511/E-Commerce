namespace AgileCommercee.Models
{
	public class SuccessVM
	{
        public int MaHd { get; set; }
        public string HoTen { get; set; }
        public string DiaChi { get; set; }
        public string Dienthoai { get; set; }
        public string CachThanhToan { get; set; }
        public string CachVanChuyen { get; set; }
        public double TongTien { get; set; }
        public List<SanPhamVM> SanPhamDaMua { get; set; }
    }
}
