using Dapper;
using TenentManagement.Models.Payment;
using TenentManagement.Services.Database;
using SkiaSharp;
using ZXing.SkiaSharp;
using ZXing;

namespace TenentManagement.Services.Payment
{
    public class PaymentQRImageService
    {
        private readonly DatabaseConnection _dbConnection;
        public PaymentQRImageService(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public List<PaymentMethodModel> GetAllAvailabeMethod(int ownerId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'P');
            parameters.Add("@OWNERID", ownerId);
            connection.Open();
            var result = connection.Query<PaymentMethodModel>(
                "SP_QRPAYMENTIMAGES",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            ).ToList();
            connection.Close();
            return result;
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

        public PaymentQRImageModel GetPaymentQRImageById(int id)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'S');
            parameters.Add("@ID", id);
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

        public PaymentQRImageModel? GetPaymentQRByOwnerIdAndPaymentId(int ownerId, int paymentMehtodId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'O');
            parameters.Add("@OWNERID", ownerId);
            parameters.Add("@PAYMENTMETHODID", paymentMehtodId);
            connection.Open();
            var result = connection.QueryFirstOrDefault<PaymentQRImageModel>(
                 "SP_QRPAYMENTIMAGES",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
                );
            return result;
        }

        public List<PaymentQRImageModel> GetAllQRImages(int ownerId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'A');
            parameters.Add("@OWNERID", ownerId);
            connection.Open();
            var result = connection.Query<PaymentQRImageModel>(
                "SP_QRPAYMENTIMAGES",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            ).ToList();
            connection.Close();
            return result;
        }

        public bool ContainsQRCode(IFormFile imageFile)
        {
            using var stream = imageFile.OpenReadStream();
            using var skBitmap = SKBitmap.Decode(stream);

            if (skBitmap == null) return false;

            var reader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
                }
            };

            var result = reader.Decode(skBitmap);
            return result != null;
        }

        public int DeleteQRImage(int id)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'D');
            parameters.Add("@ID", id);
            connection.Open();
            var result = connection.Execute("SP_QRPAYMENTIMAGES", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }

        public int UpdateQRImage(PaymentQRImageModel model)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'U');
            parameters.Add("@ID", model.Id);
            parameters.Add("@IMAGEDATA", model.ImageData);
            parameters.Add("@IMAGETYPE", model.ImageType);
            connection.Open();
            var result = connection.Execute("SP_QRPAYMENTIMAGES", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }
    }
}
