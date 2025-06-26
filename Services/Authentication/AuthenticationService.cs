using System.ComponentModel.DataAnnotations;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using TenentManagement.Models.Authentication;
using TenentManagement.Models.Authentication.EmailVerification;
using TenentManagement.Services.Bcrypt;
using TenentManagement.Services.Database;

namespace TenentManagement.Services.Authentication
{
    public class AuthenticationService
    {
        private readonly DatabaseConnection _databaseConnection;
        private readonly BCryptService _bcryptService;

        public AuthenticationService(DatabaseConnection databaseConnection, BCryptService bCryptService)
        {
            _databaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
            _bcryptService = bCryptService ?? throw new ArgumentNullException(nameof(bCryptService));
        }


        public AuthResponse RegisterUser(RegisterModel register)
        {
            var hashedPassword = _bcryptService.HashPassword(register.Password.Trim());
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'R'); // flag R for user registration in stored procedure
            parameters.Add("@USERNAME", register.UserName.ToLower());
            parameters.Add("@EMAIL", register.Email.ToLower());
            parameters.Add("@Password", hashedPassword);
            parameters.Add("@FIRSTNAME", register.FirstName.ToLower());
            parameters.Add("@MIDDLENAME", register.MiddleName == null ? null : register.MiddleName.ToLower());
            parameters.Add("@LASTNAME", register.LastName.ToLower());
            parameters.Add("@PHONENUMBER", register.PhoneNumber);

            var result = connection.QueryFirstOrDefault<AuthResponse>(
                "SP_AUTHENTICATION",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );
            connection.Close();
            if (result == null)
            {
                throw new Exception("User registration failed.");
            }
            else
            {
                return result;
            }

        }

        public AuthenticationModel? Login(LoginModel login)
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'L'); // flag L for user login in stored procedure
            parameters.Add("@EMAIL", login.Email.ToLower());
            var result = connection.QueryFirstOrDefault<AuthenticationModel>(
                "SP_AUTHENTICATION",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );
            if (result == null || result.Password == null)
            {
                throw new Exception("Invalid email or password.");
            }

            var isPasswordValid = _bcryptService.VerifyPassword(login.Password.Trim(), result.Password.Trim());

            return isPasswordValid ? result : null;
        }

        /// ----------------------------------------------------Start email verification------------------------------------------------------------------

        //Generate and store email verification token
        public EmailVerificationModel? GenerateAndStoreEmailVerificationToken(string email)
        {
            var token = new Guid().ToString();
            var expiry = DateTime.UtcNow.AddHours(168); // Token valid for 7 days

            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'I'); // flag I inserting token in stored procedure
            parameters.Add("@TOKEN", token);
            parameters.Add("@EXPIRY", expiry);
            parameters.Add("@EMAIL", email.ToLower());
            int row = connection.Execute("SP_AUTHENTICATION", parameters, commandType: CommandType.StoredProcedure);
            connection.Close();

            if (row >= 0)
            {
                return null;
            }
            else
            {
                return new EmailVerificationModel
                {
                    Token = token,
                    Email = email.ToLower(),
                    Expiry = expiry
                };
            }
        }

        //Validate token 
        public EmailVerificationModel? ValidateEmailVerificationToken(string token)
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'G'); // flag G for validating token in stored procedure
            parameters.Add("@TOKEN", token);
            var result = connection.QueryFirstOrDefault<EmailVerificationModel>(
                "SP_AUTHENTICATION",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );
            connection.Close();
            if (result == null || result.Expiry < DateTime.UtcNow)
            {
                return null; // Token is invalid or expired
            }
            return result;
        }

        //Update email verification status
        public string UpdateEmailVerificationStatus(string token)
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'V'); // flag V for updating email verification status in stored procedure
            parameters.Add("@TOKEN", token);
            int row = connection.Execute("SP_AUTHENTICATION", parameters, commandType: CommandType.StoredProcedure);
            connection.Close();
            if (row > 0)
            {
                return "Email verified successfully.";
            }
            else
            {
                throw new Exception("Email verification failed.");
            }
        }
        ///--------------------------------------------------End for email verification-----------------------------------------------------
    }
}
