using Dapper;
using TenentManagement.Models.Payment;
using TenentManagement.Services.Database;

namespace TenentManagement.Services.Payment
{
    public class PaymentQRImageService
    {
        private readonly DatabaseConnection _dbConnection;
        public PaymentQRImageService(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public int CreatePaymentQRImage(PaymentQRImageModel paymentQRImage)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'I');
            parameters.Add("@IMAGEDATA", paymentQRImage.ImageData);
            parameters.Add("@IMAGETYPE", paymentQRImage.ImageType);
            parameters.Add("@PAYMENTMETHODID", paymentQRImage.PaymentMethodId);
            parameters.Add("@OWNERID", paymentQRImage.OwnerId);
            connection.Open();
            var result = connection.Execute("SP_QRPAYMENTIMAGES", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }

        public PaymentQRImageModel GetPaymentQRImage(int ownerId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'S');
            parameters.Add("@OWNERID", ownerId);
            parameters.Add("@PAYMENTMETHODID", 2);
            connection.Open();
            var result = connection.QueryFirstOrDefault<PaymentQRImageModel>(
                "SP_QRPAYMENTIMAGES",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );
            connection.Close();
            if (result == null)
            {
                throw new Exception("QR Image not found for the specified owner and payment method.");
            }
            return result;
        }
    }
}
