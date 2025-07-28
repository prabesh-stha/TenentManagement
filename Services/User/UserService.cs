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

        public int UpdateProfile(UserModel model)
        {
            var connection = _databaseConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'U');
            parameters.Add("@ID", model.Id);
            parameters.Add("@FIRST_NAME", model.FirstName);
            parameters.Add("@LAST_NAME", model.LastName);
            if(model.MiddleName != null)
            {
                parameters.Add("@MIDDLE_NAME", model.MiddleName);
            }
            parameters.Add("@PHONE", model.PhoneNumber);
            int row = connection.Execute("SP_USERS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            return row;
        }
    }
}
