using System.ComponentModel.DataAnnotations;

namespace TenentManagement.Models.Authentication
{
    public class ChangeEmailModel
    {
        [Required]
        public int AuthId { get; set; }
        [Required(ErrorMessage = "New email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string NewEmail { get; set; } = string.Empty;
    }
}
