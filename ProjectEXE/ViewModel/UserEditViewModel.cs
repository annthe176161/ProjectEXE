using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering; // Required for SelectListItem
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectEXE.ViewModel
{
    public class UserEditViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        [Display(Name = "Active")]
        public int IsActive { get; set; }

        // This property will be used to populate the Roles dropdown in the View
        [BindNever]
        public IEnumerable<SelectListItem> RolesList { get; set; }
    }
}
