using TenentManagement.Models.Payment;

namespace TenentManagement.ViewModel
{
    public class RenterPaymentInvoiceListViewModel
    {
        public int RenterId { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public IEnumerable<PaymentInvoiceModel> PaymentInvoices { get; set; } = new List<PaymentInvoiceModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}