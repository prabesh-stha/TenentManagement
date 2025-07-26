using System.Data;
using Dapper;
using TenentManagement.Models.User;
using TenentManagement.Services.Database;

namespace TenentManagement.Services.User
{
    public class UserImageService
    {
        private readonly DatabaseConnection _dbConnection;
        public UserImageService(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public int UploadUserImage(UserImageModel model)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'U');
            parameters.Add("@IMAGEDATA", model.ImageData);
            parameters.Add("@IMAGETYPE", model.ImageType);
            parameters.Add("@USERID", model.UserId);
            connection.Open();
            int row = connection.Execute("SP_USERIMAGE", parameters, commandType: CommandType.StoredProcedure);
            connection.Close();
            return row;
        }

        public UserImageModel? GetUserImage(int userId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'G');
            parameters.Add("@USERID", userId);
            connection.Open();
            var result = connection.QueryFirstOrDefault<UserImageModel>("SP_USERIMAGE", parameters, commandType: CommandType.StoredProcedure);
            connection.Close();
            return result;
        }
    }
}
