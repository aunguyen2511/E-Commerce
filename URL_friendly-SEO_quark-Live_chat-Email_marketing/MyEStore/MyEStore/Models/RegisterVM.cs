using System.ComponentModel.DataAnnotations;

namespace MyEStore.Models
{
    public class RegisterVM
    {
        [Display(Name = "Mã khách hàng")]
        [Key]
        public string MaKh {  get; set; }
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }
        [Display(Name = "Nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage ="Mật khẩu không khớp")]
        public string MatKhauNhapLai { get; set; }
        [Display(Name = "Họ tên")]
        public string HoTen {  get; set; }
        [Display(Name = "Giới tính")]
        public bool GioiTinh {  get; set; }
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
        [Display(Name = "Số điện thoại")]
        public string? DienThoai { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Hình")]
        public string? Hinh {  get; set; }
    }
}
