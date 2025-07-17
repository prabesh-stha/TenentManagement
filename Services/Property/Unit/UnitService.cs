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
            connection.Open();
            var result = connection.Query<UnitModel>("SP_UNITS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return [.. result];
        }

        public List<UnitModel> GetAllRenterUnits(int renterId)
        {
            using var connection = _databaseConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'A');
            parameters.Add("@RENTERID", renterId);
            connection.Open();
            var result = connection.Query<UnitModel>("SP_UNITS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return [.. result];
        }

        public UnitModel? GetUnitById(int unitId)
        {
            using var connection = _databaseConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'L');
            parameters.Add("@ID", unitId);
            var result = connection.QueryFirstOrDefault<UnitModel>("SP_UNITS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }

        public int UpdateUnit(UnitModel model)
        {
            using var connection = _databaseConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'U');
            parameters.Add("@ID", model.Id);
            parameters.Add("@NAME", model.Name);
            parameters.Add("@DESCRIPTION", model.Description);
            parameters.Add("@ISVACANT", model.IsVacant);
            parameters.Add("@RENTAMOUNT", model.RentAmount);
            parameters.Add("@RENTSTARTDATE", model.RentStartDate);
            parameters.Add("@RENTENDDATE", model.RentEndDate);
            parameters.Add("@RENTERID", model.RenterId);
            parameters.Add("@PROPERTYID", model.PropertyId);
            connection.Open();
            int rowsAffected = connection.Execute("SP_UNITS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return rowsAffected;
        }

        public int DeleteUnit(int unitId)
        {
            using var connection = _databaseConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'D');
            parameters.Add("@ID", unitId);
            connection.Open();
            int rowsAffected = connection.Execute("SP_UNITS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return rowsAffected;
        }

        public RenterUnitDetailViewModel? GetRentalDetail(int unitId)
        {
            using var connection = _databaseConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'S');
            parameters.Add("@ID", unitId);
            var result = connection.QueryFirstOrDefault<RenterUnitDetailViewModel>("SP_UNITS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }

        public OwnerUnitDetailViewModel? GetOwnerUnitDetail(int unitId)
        {
            using var connection = _databaseConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'O');
            parameters.Add("@ID", unitId);
            var result = connection.QueryFirstOrDefault<OwnerUnitDetailViewModel>("SP_UNITS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }
    }
}
