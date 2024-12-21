using Azure.Identity;
using System.ComponentModel.DataAnnotations;

namespace AgileCommercee.Models
{
    public class ChangePasswordViewModel
    {
        public string UserName { get; set; }
        public string OldPassword { get; set; }
        [Required]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }
        [Required]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu phải trùng !")]
        public string ConfirmPassword { get; set; }
    }
}
