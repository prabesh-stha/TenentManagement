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

        public int CreatePayment(PaymentModel payment)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'I');
            parameters.Add("@PAIDMONTH", payment.PaidMonth);
            parameters.Add("@UNITID", payment.UnitId);
            parameters.Add("@PAYMENTDATE", payment.PaymentDate);
            parameters.Add("@AMOUNT", payment.Amount);
            parameters.Add("@INVOICEID", payment.InvoiceId);
            connection.Open();
            var result = connection.Execute("SP_PAYMENT", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }

    }
}
