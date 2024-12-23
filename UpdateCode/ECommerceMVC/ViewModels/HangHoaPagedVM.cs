using X.PagedList;

namespace ECommerceMVC.ViewModels
{
    public class HangHoaPagedVM
    {
        public IPagedList<HangHoaVM> HangHoas { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int? CurrentLoai { get; set; } // Thuộc tính này để lưu trữ loại hiện tại
        public string? CurrentQuery { get; set; } // Thuộc tính này để lưu trữ giá trị tìm kiếm hiện tại
    }
}
