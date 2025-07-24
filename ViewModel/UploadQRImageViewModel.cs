using TenentManagement.Models.Payment;

namespace TenentManagement.ViewModel
{
    public class UploadQRImageViewModel
    {
        public List<PaymentMethodModel> PaymentMethods { get; set; } = new List<PaymentMethodModel>();
        public PaymentQRImageModel PaymentQRImage { get; set; }
    }
}
