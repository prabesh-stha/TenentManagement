namespace TenentManagement.Models.Authentication
{
    public class AuthenticationModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public int PhoneNumber { get; set; }
        public string? Role { get; set; } = string.Empty;

        public bool IsVerified { get; set; } = false;
    }
}
