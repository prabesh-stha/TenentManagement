namespace TenentManagement.Models.Payment
{
    public class PaymentModel
    {
        public int Id { get; set; }
        public DateTime PaidMonth { get; set; }
        public int UnitId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public float Amount { get; set; }
        public int InvoiceId { get; set; }

    }
}
