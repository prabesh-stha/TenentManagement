namespace TenentManagement.Models.User
{
    public class UserModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int TotalOwnedProperties { get; set; } = 0;
        public int TotalOwnedUnits { get; set; } = 0;
        public int TotalRentedProperties { get; set; } = 0;
        public int TotalRentedUnits { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
