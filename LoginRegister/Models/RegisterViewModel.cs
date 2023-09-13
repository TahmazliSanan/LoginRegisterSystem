using System.ComponentModel.DataAnnotations;

namespace LoginRegister.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username cannot be empty!")]
        [StringLength(30, ErrorMessage = "Username length can be maximum 30 characters!")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password cannot be empty!")]
        [MinLength(6, ErrorMessage = "Password length can be minimum 6 characters!")]
        [MaxLength(16, ErrorMessage = "Password length can be maximum 16 characters!")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Re-Password cannot be empty!")]
        [MinLength(6, ErrorMessage = "Re-Password length can be minimum 6 characters!")]
        [MaxLength(16, ErrorMessage = "Re-Password length can be maximum 16 characters!")]
        [Display(Name = "Re-Password")]
        [Compare(nameof(Password), ErrorMessage = "Re-Password does not match your password!")]
        public string RePassword { get; set; }
    }
}