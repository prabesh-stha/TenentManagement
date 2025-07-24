using Dapper;
using TenentManagement.Models.Payment;
using TenentManagement.Services.Database;

namespace TenentManagement.Services.Payment
{
    public class PaymentProofService
    {
        private readonly DatabaseConnection _dbConnection;
        public PaymentProofService(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public int CreatePaymentProofImage(PaymentProofModel paymentProofImage)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'I');
            parameters.Add("@IMAGEDATA", paymentProofImage.ImageData);
            parameters.Add("@IMAGETYPE", paymentProofImage.ImageType);
            parameters.Add("@PAYMENTMETHODID", paymentProofImage.PaymentMethodId);
            parameters.Add("@OWNERID", paymentProofImage.OwnerId);
            parameters.Add("@INVOICEID", paymentProofImage.InvoiceId);
            connection.Open();
            var result = connection.Execute("SP_PAYMENTPROOF", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }

        public PaymentProofModel? GetPaymentProofImage(int invoiceId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'O');
            parameters.Add("@INVOICEID", invoiceId);
            connection.Open();
            var result = connection.QueryFirstOrDefault<PaymentProofModel>("SP_PAYMENTPROOF", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;

        }


    }
    }
