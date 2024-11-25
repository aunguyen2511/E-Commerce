using MyEStore.Data;
using System.ComponentModel.DataAnnotations;

namespace MyEStore.Models
{
    public class CartItem
    {
        [Key]
        public int CartId { get; set; }
        public int MaHh { get; set; }
        public string TenHh { get; set; }
        public double DonGia { get; set; }
        public string? Hinh { get; set; }
        public int SoLuong { get; set; }
        public double ThanhTien => SoLuong * DonGia;

        public virtual Cart Cart { get; set; }
        public virtual HangHoa HangHoa { get; set; }
    }
}
