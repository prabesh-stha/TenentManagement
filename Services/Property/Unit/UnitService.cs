using Dapper;
using Microsoft.Data.SqlClient;
using TenentManagement.Models.Property.Unit;
using TenentManagement.Services.Database;
using TenentManagement.ViewModel;

namespace TenentManagement.Services.Property.Unit
{
    public class UnitService
    {
        private readonly DatabaseConnection _databaseConnection;

        public UnitService(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
        }

        public void CreateUnit(UnitModel model)
        {
            var connection = _databaseConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'I');
            parameters.Add("@NAME", model.Name);
            parameters.Add("@DESCRIPTION", model.Description);
            parameters.Add("@ISVACANT", model.IsVacant);
            parameters.Add("@RENTAMOUNT", model.RentAmount);
            parameters.Add("@RENTSTARTDATE", model.RentStartDate);
            parameters.Add("@RENTENDDATE", model.RentEndDate);
            parameters.Add("@RENTERID", model.RenterId);
            parameters.Add("@PROPERTYID", model.PropertyId);
            connection.Open();
            connection.Execute("SP_UNITS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
        }

        public List<UnitModel> GetAllUnits(int propertyId)
        {
            using var connection = _databaseConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'R');
            parameters.Add("@PROPERTYID", propertyId);
            var result = connection.Query<UnitModel>("SP_UNITS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return [.. result];
        }

        public RenterDetailViewModel? GetRentalDetail(int unitId)
        {
            using var connection = _databaseConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'S');
            parameters.Add("@ID", unitId);
            var result = connection.QueryFirstOrDefault<RenterDetailViewModel>("SP_UNITS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }
    }
}
