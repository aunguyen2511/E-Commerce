using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyEStore.Models
{
    public class LoginVM
    {
        // đăng nhập trên đó sai 4 5 lần bị gì thì xài session nhưng
        // nên lưu trong DB do lưu như vậy thì mới block được chứ session ko block được
        [Key]
        [Display(Name ="Tên đăng nhập")]
        [MaxLength(20)]
        public string UserName { get; set; }
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
