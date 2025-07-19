using TenentManagement.Models.Payment;

namespace TenentManagement.ViewModel
{
    public class PaymentInvoiceListViewModel
    {
        public int UnitId { get; set; }
        public IEnumerable<PaymentInvoiceModel> PaymentInvoices { get; set; } = new List<PaymentInvoiceModel>();
    }
}
