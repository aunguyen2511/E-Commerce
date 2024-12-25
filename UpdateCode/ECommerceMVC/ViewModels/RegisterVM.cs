using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
    public class RegisterVM
    {
        [Display(Name ="Tên đăng nhập")]
        [Required(ErrorMessage ="*")]
        [MaxLength(20, ErrorMessage ="Tối đa 20 kí tự ")]
        public string? MaKh { get; set; }
        
        [Display(Name ="Mật khẩu")]
        [Required(ErrorMessage = "*")]
        [MaxLength(50, ErrorMessage = "Tối đa 50 kí tự ")]
        public string? MatKhau { get; set; }

        [Required(ErrorMessage = "Confirm Mật Khẩu")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Mật Khẩu chưa  khớp.")]
        public string? ConfirmMatKhau { get; set; }

        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; }

        [Display(Name = "Giới tính")]
        public bool GioiTinh { get; set; } = true;

        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; }

        [Display(Name = "Địa chỉ")]
        [MaxLength(60, ErrorMessage = "Tối đa 60 kí tự ")]
        public string? DiaChi { get; set; }

        [Display(Name = "Điện thoại")]

        [MaxLength(24, ErrorMessage = "Tối đa 24 kí tự ")]
        [RegularExpression(@"^(0[9875]\d{8}|(\+?\d{1,3})?[-.\s]?\(?\d{1,4}?\)?[-.\s]?\d{1,4}[-.\s]?\d{1,9})$", ErrorMessage = "Số điện thoại không hợp lệ.")]

        public string? DienThoai { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage ="Chưa đúng định dạng email")]
        public string Email { get; set; } = null!;

        [Display(Name = "Hình")]
        public string? Hinh { get; set; }
    }
}
