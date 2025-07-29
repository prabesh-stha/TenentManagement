using System.ComponentModel.DataAnnotations;

namespace TenentManagement.Models.User
{
    public class UserModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(11, MinimumLength = 10, ErrorMessage = "Enter a valid phone number.")]
        public string PhoneNumber { get; set; } = string.Empty;
        public int TotalOwnedProperties { get; set; } = 0;
        public int TotalOwnedUnits { get; set; } = 0;
        public int TotalRentedProperties { get; set; } = 0;
        public int TotalRentedUnits { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public UserImageModel? UserImage { get; set; }
    }
}
