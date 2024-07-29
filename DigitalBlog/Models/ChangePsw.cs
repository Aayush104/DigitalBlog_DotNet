using System.ComponentModel.DataAnnotations;

namespace DigitalBlog.Models
{
	public class ChangePsw
	{

		[DataType(DataType.Password)]
		[Required(ErrorMessage ="Please Enter Your Current Password")]
		[Display(Name ="Current Password")]
		public string CurrentPassword { get; set; } = null!;
		[DataType(DataType.Password)]
		[Required(ErrorMessage = "Please Enter Your New Password")]
		[Display(Name = "New Password")]
		public string NewPassword { get; set; } = null!;
		[DataType(DataType.Password)]
		[Required(ErrorMessage = "Please Enter Your Current Password")]
		[Display(Name = "Confirm Password")]
		[Compare("NewPassword",ErrorMessage = "Confirm Password Doesnot matched")]
public string ConfirmPassword { get; set; } = null!;
	}
}

