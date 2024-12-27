using AgileCommercee.Entities;
using System.ComponentModel;

namespace AgileCommercee.Models
{
    public class KetQuaTimKiemVM
    {
        [DisplayName("Product ID")]
        public int MaHh { get; set; }
        [DisplayName("Product Name")]
        public string TenHh { get; set; } = null!;
        [DisplayName("Category ID")]
        public int MaLoai { get; set; }
        [DisplayName("Unit Description")]
        public string? MoTaDonVi { get; set; }
        [DisplayName("Unit Price")]
        public double? DonGia { get; set; }
        [DisplayName("Image")]
        public string? Hinh { get; set; }
        [DisplayName("Manufaturing date")]
        public DateTime NgaySx { get; set; }
        [DisplayName("Discount")]
        public double GiamGia { get; set; }
        [DisplayName("Number of views")]
        public int SoLanXem { get; set; }
        [DisplayName("Description")]
        public string? MoTa { get; set; }
        [DisplayName("Supplier ID")]
        public string MaNcc { get; set; } = null!;

        public virtual ICollection<BanBe> BanBes { get; set; } = new List<BanBe>();

        public virtual ICollection<ChiTietHd> ChiTietHds { get; set; } = new List<ChiTietHd>();

        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

        [DisplayName("Category")]
        public virtual Loai MaLoaiNavigation { get; set; } = null!;
        [DisplayName("Supplier")]
        public virtual NhaCungCap MaNccNavigation { get; set; } = null!;
    }
}
