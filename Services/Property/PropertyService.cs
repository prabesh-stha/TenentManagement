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
        private readonly PropertyImageService _propertyImageService;
        
        public PropertyService(DatabaseConnection databaseConnection, PropertyImageService propertyImageService)
        {
            _databaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
            _propertyImageService = propertyImageService ?? throw new ArgumentNullException(nameof(propertyImageService));


        }

        public List<PropertyTypeModel> GetAllPropertyTypes()
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'R');
            var result = connection.Query<PropertyTypeModel>("SP_PROPERTY_TYPES", parameters, commandType: CommandType.StoredProcedure);
            connection.Close();
            return [.. result];
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
            parameters.Add("@TYPE", property.Type);
            if (property.PropertyImage != null)
            {
                parameters.Add("@IMAGEDATA", property.PropertyImage.ImageData);
                parameters.Add("@IMAGETYPE", property.PropertyImage.ImageType);
            }
            parameters.Add("@STATUS", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

            connection.Execute("SP_PROPERTY", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            int row = parameters.Get<int>("@STATUS");
            if (row == 1)
            {
                return "success";
            }
            else
            {
                throw new Exception("Failed to create property.");
            }
        }

        public async Task<List<PropertyModel>> GetAllProperty(int userId)
        {
            using var connection = _databaseConnection.GetConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'R');
            parameters.Add("@USERID", userId);

            var result = await connection.QueryAsync<PropertyModel>("SP_PROPERTY", parameters, commandType: CommandType.StoredProcedure);

            if (result.Count() != 0)
            {
                foreach (var property in result)
                {


                    var images = _propertyImageService.GetPropertyImage(property.Id);
                    property.PropertyImage = images;
                }
            }

            return [.. result];
        }

        public async Task<List<PropertyModel>> GetAllRentedProperty(int userId)
        {
            using var connection = _databaseConnection.GetConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'T');
            parameters.Add("@USERID", userId);

            var result = await connection.QueryAsync<PropertyModel>("SP_PROPERTY", parameters, commandType: CommandType.StoredProcedure);
            if (result.Count() != 0)
            {
                foreach (var property in result)
                {


                    var images = _propertyImageService.GetPropertyImage(property.Id);
                    property.PropertyImage = images;
                }
            }

            return [.. result];
        }

        public PropertyDetailViewModel GetPropertyAndUnitDetail(int propertyId, int? renterId)
        {
            using var connection = _databaseConnection.GetConnection();
            connection.Open();
            var parametersProperty = new DynamicParameters();
            var parametersUnit = new DynamicParameters();
            var parametersImage = new DynamicParameters();
            parametersProperty.Add("@FLAG", 'S');
            parametersProperty.Add("@ID", propertyId);
            parametersUnit.Add("@PROPERTYID", propertyId);
            if(renterId == null)
            {
                parametersUnit.Add("@FLAG", 'R');
            }
            else
            {
                parametersUnit.Add("@FLAG", 'M');
                parametersUnit.Add("@RENTERID", renterId);
            }
            parametersImage.Add("@FLAG", 'G');
            parametersImage.Add("@PROPERTYID", propertyId);
            var property = connection.QueryFirstOrDefault<PropertyModel>("SP_PROPERTY", parametersProperty, commandType: CommandType.StoredProcedure);
            var units = connection.Query<UnitModel>("SP_UNITS", parametersUnit, commandType: CommandType.StoredProcedure).ToList();
            var propertyImage = connection.QueryFirstOrDefault<PropertyImageModel>("SP_PROPERTYIMAGE", parametersImage, commandType: CommandType.StoredProcedure);
                property.PropertyImage = propertyImage;
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
            parameter.Add("@STATUS", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
            connection.Execute("SP_PROPERTY", parameter, commandType: CommandType.StoredProcedure);
            connection.Close();
            return parameter.Get<int>("@STATUS");
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
            if(property.PropertyImage != null)
            {
                parameter.Add("@IMAGEDATA", property.PropertyImage.ImageData);
                parameter.Add("@IMAGETYPE", property.PropertyImage.ImageType);
            }
            parameter.Add("@STATUS", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
     
            connection.Execute("SP_PROPERTY", parameter, commandType: CommandType.StoredProcedure);
            connection.Close();
            return parameter.Get<int>("@STATUS");
        }
    }
}
