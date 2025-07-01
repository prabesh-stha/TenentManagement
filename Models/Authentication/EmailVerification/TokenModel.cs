namespace TenentManagement.Models.Authentication.EmailVerification
{
    public class TokenModel
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiry { get; set; } = DateTime.Now;

    }
}
