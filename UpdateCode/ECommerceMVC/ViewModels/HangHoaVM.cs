namespace ECommerceMVC.ViewModels
{
	public class HangHoaVM
	{
        public int MaHh { get; set; }
        public string TenHh { get; set; } = null!;
        public string? TenAlias { get; set; }
        public int MaLoai { get; set; }
        public string? MoTaDonVi { get; set; }
        public double? DonGia { get; set; }
        public string? Hinh { get; set; }
        public DateTime NgaySx { get; set; }
        public double GiamGia { get; set; }
        public string? MoTa { get; set; }
        public string MaNcc { get; set; } = null!;
    }

	public class ChiTietHangHoaVM
	{
        public int MaHh { get; set; }
        public string TenHH { get; set; }
        public string Hinh { get; set; }
        public double DonGia { get; set; }
        public string MoTaNgan { get; set; }
        public string TenLoai { get; set; }
        public string ChiTiet { get; set; }
        public int DiemDanhGia { get; set; }
        public int SoLuongTon { get; set; }
    }
}
