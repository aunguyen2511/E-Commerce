using System.ComponentModel.DataAnnotations;

namespace AgileCommercee.Models
{
    public class ResetPasswordViewModel
    {
        public string username { get; set; }
        public string email { get; set; }
        public string NewPassword { get; set; }
        [Required]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu phải trùng !")]
        public string ConfirmPassword { get; set; }
    }
}
