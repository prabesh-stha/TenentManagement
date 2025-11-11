using Dapper;
using TenentManagement.Models.Property.Utility;
using TenentManagement.Services.Database;

namespace TenentManagement.Services.Property.UtilityBill
{
    public class UtilityBillInvoiceService
    {
        private readonly DatabaseConnection _dbConnection;
        public UtilityBillInvoiceService(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public int CreateUtilityBillInvoice(UtilityBillInvoiceModel utilityBillInvoiceModel)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'I');
            parameters.Add("@INVOICEID", utilityBillInvoiceModel.InvoiceId);
            parameters.Add("@UTILITYTYPEID", utilityBillInvoiceModel.UtilityTypeId);
            parameters.Add("@TOTALUNIT", utilityBillInvoiceModel.TotalUnit);
            parameters.Add("@CONSUMEDUNIT", utilityBillInvoiceModel.ConsumedUnit);
            parameters.Add("@AMOUNT", utilityBillInvoiceModel.Amount);
            parameters.Add("@UTILITYID", utilityBillInvoiceModel.UtilityId);
            connection.Open();
            int row = connection.Execute("SP_UTILITYBILLINVOICE", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return row;
        }

        public int DeleteUtilityBillInvoice(int? id = null, int? invoiceId = null)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'D');
            parameters.Add("@INVOICEID", invoiceId);
            parameters.Add("@ID", id);
            connection.Open();
            int row = connection.Execute("SP_UTILITYBILLINVOICE", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return row;
        }

        public int UpdateUtilityBillInvoice(UtilityBillInvoiceModel utilityBillInvoiceModel)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", "U");
            parameters.Add("@ID", utilityBillInvoiceModel.Id);
            parameters.Add("@CONSUMEDUNIT", utilityBillInvoiceModel.ConsumedUnit);
            parameters.Add("@TOTALUNIT", utilityBillInvoiceModel.TotalUnit);
            parameters.Add("@AMOUNT", utilityBillInvoiceModel.Amount);
            parameters.Add("@UTILITYID", utilityBillInvoiceModel.UtilityId);
            connection.Open();
            int row = connection.Execute("SP_UTILITYBILLINVOICE", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return row;
        }

        public List<UtilityBillInvoiceModel> GetUtilityBillInvoiceByInoviceId(int invoiceId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'P');
            parameters.Add("@INVOICEID", invoiceId);
            connection.Open();
            var result = connection.Query<UtilityBillInvoiceModel>("SP_UTILITYBILLINVOICE", parameters, commandType:System.Data.CommandType.StoredProcedure).ToList();
            connection.Close();
            return result;
        }

        public int? GetTotalUnit(int unitId, int typeId, int invoiceId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'C');
            parameters.Add("@UNITID", unitId);
            parameters.Add("@UTILITYTYPEID", typeId);
            parameters.Add("@INVOICEID", invoiceId);
            var result = connection.QueryFirstOrDefault<int?>("SP_UTILITYBILLINVOICE", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }
    }
}
