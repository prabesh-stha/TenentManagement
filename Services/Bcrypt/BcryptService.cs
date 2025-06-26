namespace TenentManagement.Services.Bcrypt
{

        public class BCryptService
        {
            public string HashPassword(string password)
            {
                return BCrypt.Net.BCrypt.HashPassword(password);
            }
            public bool VerifyPassword(string password, string hashedPassword)
            {
                if (string.IsNullOrEmpty(hashedPassword))
                    return false;
                return BCrypt.Net.BCrypt.Verify(
                    text: password.Trim(),
                    hash: hashedPassword.Trim()
                );
            }
        }

}
