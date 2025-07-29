using TenentManagement.Models.Payment;

namespace TenentManagement.ViewModel
{
    public class PaymentInvoiceListViewModel
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public bool IsOwner { get; set; } = false;
        public IEnumerable<PaymentInvoiceModel> PaymentInvoices { get; set; } = new List<PaymentInvoiceModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
