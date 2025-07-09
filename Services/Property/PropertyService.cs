using TenentManagement.Services.Database;
using TenentManagement.Models.Property;
using Dapper;
using System.Data;
using TenentManagement.ViewModel;
using TenentManagement.Models.Property.Unit;
using System.Net;
using System;

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

            return [.. result];
        }

        public List<PropertyModel> GetAllRentedProperty(int userId)
        {
            using var connection = _databaseConnection.GetConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'T');
            parameters.Add("@USERID", userId);

            var result = connection.Query<PropertyModel>("SP_PROPERTY", parameters, commandType: CommandType.StoredProcedure);

            return [.. result];
        }

        public PropertyDetailViewModel GetPropertyAndUnitDetail(int propertyId)
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parametersProperty = new DynamicParameters();
            var parametersUnit = new DynamicParameters();
            parametersProperty.Add("@FLAG", 'S');
            parametersProperty.Add("@ID", propertyId);
            parametersUnit.Add("@FLAG", 'R');
            parametersUnit.Add("@PROPERTYID", propertyId);
            var property = connection.QueryFirstOrDefault<PropertyModel>("SP_PROPERTY", parametersProperty, commandType: CommandType.StoredProcedure);
            var units = connection.Query<UnitModel>("SP_UNITS", parametersUnit, commandType: CommandType.StoredProcedure).ToList();
            connection.Close();
            PropertyDetailViewModel propertyDetailViewModel = new()
            {
                Property = property,
                Units = units.ToList()
            };
            return propertyDetailViewModel;
        }

        public PropertyModel? GetPropertyDetail(int propertyId)
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parametersProperty = new DynamicParameters();
            parametersProperty.Add("@FLAG", 'S');
            parametersProperty.Add("@ID", propertyId);
            var property = connection.QueryFirstOrDefault<PropertyModel>("SP_PROPERTY", parametersProperty, commandType: CommandType.StoredProcedure);
            connection.Close();
            return property;
        }

        public int DeleteProperty(int propertyId)
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameter = new DynamicParameters();
            parameter.Add("@FLAG", 'D');
            parameter.Add("@ID", propertyId);
            int row = connection.Execute("SP_PROPERTY", parameter, commandType: CommandType.StoredProcedure);
            connection.Close();
            return row;
        }

        public int UpdateProperty(PropertyModel property)
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameter = new DynamicParameters();
            parameter.Add("@FLAG", 'U');
            parameter.Add("@ID", property.Id);
            parameter.Add("@NAME", property.Name);
            parameter.Add("@ADDRESS", property.Address);
            parameter.Add("@LATITUDE", property.Latitude);
            parameter.Add("@LONGITUDE", property.Longitude);
            parameter.Add("@DESCRIPTION", property.Description);
            int row = connection.Execute("SP_PROPERTY", parameter, commandType: CommandType.StoredProcedure);
            connection.Close();
            return row;
        }
    }
}
