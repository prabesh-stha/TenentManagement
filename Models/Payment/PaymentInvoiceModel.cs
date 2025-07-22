namespace TenentManagement.Models.Payment
{
    public class PaymentInvoiceModel
    {
        public int Id { get; set; }
        public int RenterId { get; set; }
        public string RenterName { get; set; } = string.Empty;
        public int UnitId { get; set; }
        public DateTime FromMonth { get; set; }

        public DateTime ToMonth { get; set; }
        public DateTime DueDate { get; set; }
        public float AmountDue { get; set; }
        public float AmountPerMonth { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public int PaymentMethodId { get; set; } = 1;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public int StatusId { get; set; } = 3;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public List<DateTime> AvailableMonth { get; set; } = new List<DateTime>();

        public IEnumerable<PaymentMethodModel> PaymentMethods { get; set; } = new List<PaymentMethodModel>();
        public IEnumerable<PaymentStatusModel> PaymentStatuses { get; set; } = new List<PaymentStatusModel>();

    }
}
