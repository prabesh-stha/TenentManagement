using Dapper;
using TenentManagement.Models.User;
using TenentManagement.Services.Database;

namespace TenentManagement.Services.User
{
    public class UserService
    {
        private readonly DatabaseConnection _databaseConnection;

        public UserService(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
        }

        public UserModel GetProfile(int userId)
        {
            var connection = _databaseConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'P');
            parameters.Add("@ID", userId);
            connection.Open();
            var result = connection.QueryFirstOrDefault<UserModel>("SP_USERS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            if (result == null)
            {
                throw new Exception("User not found");
            }
            return result;
        }
    }
}
