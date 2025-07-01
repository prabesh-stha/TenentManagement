using System.ComponentModel.DataAnnotations;

namespace TenentManagement.Models.Authentication
{
    public class ResetPasswordModel
    {
        public string Token { get; set; } = string.Empty;

        public DateTime Expiry { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
ErrorMessage = "Password must contain uppercase, lowercase, digit, and special character.")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Password must match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
