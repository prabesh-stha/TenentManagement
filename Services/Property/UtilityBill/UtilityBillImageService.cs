using Dapper;
using TenentManagement.Models.Property.Utility;
using TenentManagement.Services.Database;

namespace TenentManagement.Services.Property.UtilityBill
{
    public class UtilityBillImageService
    {
        private readonly DatabaseConnection _dbConnection;
        public UtilityBillImageService(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public int UploadUtilityBillImage(UtilityBillImageModel model)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'I');
            parameters.Add("@IMAGEDATA", model.ImageData);
            parameters.Add("@IMAGETYPE", model.ImageType);
            parameters.Add("@UTILITYBILLID", model.UtilityBillId);
            connection.Open();
            int row = connection.Execute("SP_UTILITYBILLIMAGE", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return row;
        }

        public int UpdateUtilityBillImage(UtilityBillImageModel model)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'U');
            parameters.Add("@ID", model.Id);
            parameters.Add("@IMAGEDATA", model.ImageData);
            parameters.Add("@IMAGETYPE", model.ImageType);
            connection.Open();
            int row = connection.Execute("SP_UTILITYBILLIMAGE", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return row;
        }

        public int DeleteUtilityBillImage(int id)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'D');
            parameters.Add("@ID", id);
            connection.Open();
            int row = connection.Execute("SP_UTILITYBILLIMAGE", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return row;
        }

        public UtilityBillImageModel? GetUtilityBillImageById(int id = 0, int? utilityBillId = null)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'G');
            if(utilityBillId.HasValue)
                parameters.Add("@UTILITYBILLID", utilityBillId.Value);
            else
                parameters.Add("@ID", id);
            connection.Open();
            var result = connection.QueryFirstOrDefault<UtilityBillImageModel>("SP_UTILITYBILLIMAGE", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }
    }
}
