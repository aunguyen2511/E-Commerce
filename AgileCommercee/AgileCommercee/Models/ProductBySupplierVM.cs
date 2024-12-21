namespace AgileCommercee.Models
{
    public class ProductBySupplierVM
    {
        public int HangHoaMaHh { get; set; }
        public string HangHoaTenHh { get; set; }
        public string MaLoaiNavigation { get; set; }
        public double? DonGia { get; set; }
        public string MaNccNavigation { get; set; }
        public string Status
        {
            get
            {
                if (DonGia < 100) return "Cheap";
                if (DonGia >= 100 && DonGia < 1000) return "Normal";
                else return "Expensive";
            }
        }
    }
    public class ProductByCategoryVM
    {
        public int HangHoaMaHh { get; set; }
        public string HangHoaTenHh { get; set; }
        public string MaLoaiNavigation { get; set; }
        public double? DonGia { get; set; }
        public string MaNccNavigation { get; set; }
        public string Status
        {
            get
            {
                if (DonGia < 100) return "Cheap";
                if (DonGia >= 100 && DonGia < 1000) return "Normal";
                else return "Expensive";
            }
        }
    }
}
