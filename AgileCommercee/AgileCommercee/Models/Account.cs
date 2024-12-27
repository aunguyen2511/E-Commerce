using System.ComponentModel.DataAnnotations;

namespace AgileCommercee.Models
{
    public class Account
    {
        public string MaKh { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string? Hinh { get; set; }
        public string Password { get; set; }
    }
}
