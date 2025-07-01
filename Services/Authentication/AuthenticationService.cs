using System.ComponentModel.DataAnnotations;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using TenentManagement.Models.Authentication;
using TenentManagement.Models.Authentication.EmailVerification;
using TenentManagement.Services.Bcrypt;
using TenentManagement.Services.Database;
using static Azure.Core.HttpHeader;

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
                commandType: CommandType.StoredProcedure
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
                commandType: CommandType.StoredProcedure
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
        public TokenModel? GenerateAndStoreEmailVerificationToken(string email)
        {
            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.Now.AddHours(168); // Token valid for 7 days
            return StoreToken(token, expiry, email.ToLower());
        }



        //Update email verification status
        public string UpdateEmailVerificationStatus(string token, string email)
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'V'); // flag V for updating email verification status in stored procedure
            parameters.Add("@TOKEN", token);
            parameters.Add("@EMAIL", email);
            int row = connection.Execute("SP_AUTHENTICATION", parameters, commandType: CommandType.StoredProcedure);
            connection.Close();
            if (row > 0)
            {
                return "success";
            }
            else
            {
                throw new Exception("Email verification failed.");
            }
        }
        ///--------------------------------------------------End for email verification-----------------------------------------------------
        ///

        public TokenModel? StoreToken(string token, DateTime expiry, string email)
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'I'); // flag I inserting token in stored procedure
            parameters.Add("@TOKEN", token);
            parameters.Add("@EXPIRY", expiry);
            parameters.Add("@EMAIL", email.ToLower());
            int row = connection.Execute("SP_AUTHENTICATION", parameters, commandType: CommandType.StoredProcedure);
            connection.Close();

            if (row <= 0)
            {
                return null;
            }
            else
            {
                return new TokenModel
                {
                    Token = token,
                    Email = email.ToLower(),
                    Expiry = expiry
                };
            }
        }

        //Validate token 
        public TokenModel? ValidateToken(string token)
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'G'); // flag G for validating token in stored procedure
            parameters.Add("@TOKEN", token);

            var result = connection.QueryFirstOrDefault<TokenModel>(
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

        public TokenModel? GenerateAndStorePasswordToken(string email)
        {
            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.Now.AddHours(1); // Token valid for 1 days
            return StoreToken(token, expiry, email.ToLower());
        }

        public string ResetPassword(ResetPasswordModel model)
        {
            var hashedPassword = _bcryptService.HashPassword(model.Password);
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", "U");
            parameters.Add("@TOKEN", model.Token);
            parameters.Add("@PASSWORD", hashedPassword);

            var row = connection.Execute("SP_AUTHENTICATION", parameters, commandType: CommandType.StoredProcedure);
            connection.Close();
            if (row > 0)
            {
                return "success";
            }
            else
            {
                return "error";
            }
        }
    }
}
