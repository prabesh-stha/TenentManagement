namespace TenentManagement.Models.Payment
{
    public class InvoiceCreationAlertModel
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public string RenterName { get; set; } = string.Empty;

        public DateTime PaidTo { get; set; }
    }
}
