namespace TenentManagement.Models.Authentication.EmailVerification
{
    public class EmailVerificationModel
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiry { get; set; } = DateTime.Now;

    }
}
