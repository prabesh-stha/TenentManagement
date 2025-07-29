namespace TenentManagement.Models.Payment
{
    public class DueDateAlertModel
    {
        public int InvoiceId { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string OwnerName { get; set;} = string.Empty;
        public int RenterId { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsExpired { get; set; }
    }
}
