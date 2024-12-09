using MyEStore.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyEStore.Models
{
    public class CartItem
    {
        [Key]
        public int CartId { get; set; }
        [Display(Name = "Mã hàng hoá")]
        public int MaHh { get; set; }
        [Display(Name = "Tên hàng hoá")]
        public string TenHh { get; set; }
        [Display(Name = "Đơn giá")]
        public double DonGia { get; set; }
        [Display(Name = "Hình")]
        public string? Hinh { get; set; }
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; }
        [Display(Name = "Thành tiền")]
        public double ThanhTien => SoLuong * DonGia;
        public virtual Cart Cart { get; set; }
        public virtual HangHoa HangHoa { get; set; }
    }
}