namespace TenentManagement.Models.Property.Utility
{
    public class UtilityBillInvoiceModel
    {
        public int Id { get; set; }
        public int UtilityTypeId { get; set; }
        public string? UtilityTypeName { get; set; } = null;
        public int InvoiceId { get; set; }
        public int ConsumedUnit { get; set; }
        public float Amount { get; set; }

    }
}