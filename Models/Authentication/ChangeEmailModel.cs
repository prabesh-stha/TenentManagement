using System.ComponentModel.DataAnnotations;

namespace TenentManagement.Models.Authentication
{
    public class ChangeEmailModel
    {
        public string Token { get; set; } = string.Empty;

        public DateTime Expiry { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "New email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string NewEmail { get; set; } = string.Empty;

        public string OldEmail { get; set; } = string.Empty;
    }
}
