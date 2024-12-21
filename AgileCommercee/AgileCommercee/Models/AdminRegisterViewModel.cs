using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace AgileCommercee.Models
{
    public class AdminRegisterViewModel
    {
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Role (0 for Role Admin and Null for Customer)")]
        public int Roles { get; set; }

        [Display(Name = "Picture")]
        public string? Hinh { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Mật khẩu phải trùng !")]
        public string ConfirmPassword { get; set; }


    }
}
