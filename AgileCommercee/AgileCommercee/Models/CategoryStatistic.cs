namespace AgileCommercee.Models
{
    public class CategoryStatistic
    {
        public string MaLoaiNavigation { get; set; }
        public int NumOfProduct { get; set; }
    }
    public class SupplierStatistic
    {
        public string MaNccNavigation { get; set; }
        public int NumOfProduct { get; set; }
    }
    public class SupplierCategoryStatistic
    {
        public string MaNccNavigation { get; set; }
        public string MaLoaiNavigation { get; set; }
        public int NumOfProduct { get; set; }
    }
}
