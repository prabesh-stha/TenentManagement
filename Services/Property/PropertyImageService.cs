using Dapper;
using System.Data;
using TenentManagement.Models.Property;
using TenentManagement.Services.Database;

namespace TenentManagement.Services.Property
{
    public class PropertyImageService
    {
        private readonly DatabaseConnection _dbConnection;
        public PropertyImageService(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public int UploadPropertyImage(PropertyImageModel model)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'U');
            parameters.Add("@IMAGEDATA", model.ImageData);
            parameters.Add("@IMAGETYPE", model.ImageType);
            parameters.Add("@PROPERTYID", model.PropertyId);
            connection.Open();
            int row = connection.Execute("SP_PROPERTYIMAGE", parameters, commandType: CommandType.StoredProcedure);
            connection.Close();
            return row;
        }

        public PropertyImageModel? GetPropertyImage(int propertyId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'G');
            parameters.Add("@PROPERTYID", propertyId);
            connection.Open();
            var result = connection.QueryFirstOrDefault<PropertyImageModel>("SP_PROPERTYIMAGE", parameters, commandType: CommandType.StoredProcedure);
            connection.Close();
            return result;
        }
    }
}
