using Dapper;
using TenentManagement.Models.Payment;
using TenentManagement.Services.Database;

namespace TenentManagement.Services.Payment
{
    public class PaymentService
    {
        private readonly DatabaseConnection _dbConnection;
        public PaymentService(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public List<PaymentModel> GetAllPaymentByUnit(int unitId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'A');
            parameters.Add("@UNITID", unitId);
            connection.Open();
            var result = connection.Query<PaymentModel>("SP_PAYMENT", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return [..result];
        }
    }
}
