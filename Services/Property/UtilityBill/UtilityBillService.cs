using Dapper;
using System.Data.Common;
using System.Reflection.Metadata;
using TenentManagement.Models.Property.Utility;
using TenentManagement.Services.Database;

namespace TenentManagement.Services.Property.UtilityBill
{
    public class UtilityBillService
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly UtilityBillImageService _utilityBillImageService;
        public UtilityBillService(DatabaseConnection dbConnection, UtilityBillImageService utilityBillImageService)
        {
            _dbConnection = dbConnection;
            _utilityBillImageService = utilityBillImageService;
        }

        public List<UtilityTypeModel> GetUtilityBills()
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'A');
            connection.Open();
            var result = connection.Query<UtilityTypeModel>("SP_UTILITYTYPE", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
            connection.Close();
            return result;
        }

        public int UploadUtilityBill(UtilityBillModel model)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'I');
            parameters.Add("@UTILITYTYPE", model.UtilityTypeId);
            parameters.Add("@PROPERTYID", model.PropertyId);
            parameters.Add("@USERID", model.UserId);
            parameters.Add("@MONTH", model.Month);
            parameters.Add("@TOTALUNIT", model.TotalUnit);
            parameters.Add("@CONSUMEDUNIT", model.ConsumedUnit);
            parameters.Add("@AMOUNT", model.Amount);
            parameters.Add("@ISPAID", model.IsPaid);
            if (model.UtilityBillImage != null)
            {
                parameters.Add("@IMAGEDATA", model.UtilityBillImage.ImageData);
                parameters.Add("@IMAGETYPE", model.UtilityBillImage.ImageType);
            }
            parameters.Add("@STATUS", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
            connection.Open();
            connection.Execute("SP_UTILITYBILLS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return parameters.Get<int>("@STATUS");
        }

        public int UpdateUtilityBill(UtilityBillModel model)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'U');
            parameters.Add("@ID", model.Id);
            parameters.Add("@UTILITYTYPE", model.UtilityTypeId);
            parameters.Add("@MONTH", model.Month);
            parameters.Add("@TOTALUNIT", model.TotalUnit);
            parameters.Add("@CONSUMEDUNIT", model.ConsumedUnit);
            parameters.Add("@AMOUNT", model.Amount);
            parameters.Add("@ISPAID", model.IsPaid);
            if (model.UtilityBillImage != null)
            {
                parameters.Add("@IMAGEDATA", model.UtilityBillImage.ImageData);
                parameters.Add("@IMAGETYPE", model.UtilityBillImage.ImageType);
            }
            parameters.Add("@STATUS", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
            connection.Open();
            connection.Execute("SP_UTILITYBILLS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return parameters.Get<int>("@STATUS");
        }

        public List<UtilityBillModel> GetAllUtilityBill(int userId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'A');
            parameters.Add("@USERID", userId);
            connection.Open();
            var result = connection.Query<UtilityBillModel>("SP_UTILITYBILLS", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
            if (result.Count() != 0)
            {
                foreach (var bill in result)
                {
                    var images = _utilityBillImageService.GetUtilityBillImageById(0, bill.Id);
                    bill.UtilityBillImage = images;
                }
            }
            connection.Close();
            return result;
        }

        public UtilityBillModel? GetUtilityBillById(int id)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'G');
            parameters.Add("@ID", id);
            connection.Open();
            var result = connection.QueryFirstOrDefault<UtilityBillModel>("SP_UTILITYBILLS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }

        public int DeleteUtilityBill(int id)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'D');
            parameters.Add("@ID", id);
            parameters.Add("@STATUS", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
            connection.Open();
            connection.Execute("SP_UTILITYBILLS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return parameters.Get<int>("@STATUS");
            ;
        }

        public List<UtilityLatestMonthModel> GetLatestMonth()
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'L');
            connection.Open();
            var result = connection.Query<UtilityLatestMonthModel>("SP_UTILITYBILLS", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
            connection.Close();
            return result;
        }
    }
}
