using TenentManagement.Services.Database;
using TenentManagement.Models.Property;
using Dapper;
using System.Data;

namespace TenentManagement.Services.Property
{
    public class PropertyService
    {
        private readonly DatabaseConnection _databaseConnection;
        
        public PropertyService(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
            
        }

        public string CreateProperty(PropertyModel property)
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'I');
            parameters.Add("@NAME", property.Name);
            parameters.Add("@ADDRESS", property.Address);
            parameters.Add("@LATITUDE", property.Latitude);
            parameters.Add("@LONGITUDE", property.Longitude);
            parameters.Add("@DESCRIPTION", property.Description);
            parameters.Add("@USERID", property.UserId);
            var row = connection.Execute("SP_PROPERTY", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            if (row >= 1)
            {
                return "success";
            }
            else
            {
                throw new Exception("Failed to create property.");
            }
        }

        public List<PropertyModel> GetAllProperty(int userId)
        {
            using var connection = _databaseConnection.GetConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'R');
            parameters.Add("@USERID", userId);

            var result = connection.Query<PropertyModel>("SP_PROPERTY", parameters, commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public PropertyModel? GetPropertyDetail(int propertyId)
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'S');
            parameters.Add("@ID", propertyId);
            var result = connection.QueryFirstOrDefault<PropertyModel>("SP_PROPERTY", parameters, commandType: CommandType.StoredProcedure);
            connection.Close();
            return result;
        }
    }
}
