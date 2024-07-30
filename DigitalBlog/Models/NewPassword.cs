using System.ComponentModel.DataAnnotations;

namespace DigitalBlog.Models
{
    public class NewPassword
    {
        
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please Enter Your New Password")]
        [Display(Name = "New Passwords")]
        public string NewPasswords { get; set; } = null!;
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please Enter Your Current Password")]
        [Display(Name = "Confirm Password")]
        [Compare("NewPasswords", ErrorMessage = "Confirm Password Doesnot matched")]
        public string ConfirmPassword { get; set; } = null!;


        public string email { get; set; } = null!;
    }
}


