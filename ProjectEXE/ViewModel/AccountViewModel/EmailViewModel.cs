using System.ComponentModel.DataAnnotations;

namespace ProjectEXE.ViewModel.AccountViewModel
{
    public class EmailViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
