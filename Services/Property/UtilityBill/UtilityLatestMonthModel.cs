namespace TenentManagement.Services.Property.UtilityBill
{
    public class UtilityLatestMonthModel
    {
        public int UtilityType { get; set; } 
        public int PropertyId { get; set; }
        public DateTime? LatestMonth {  get; set; } 
    }
}
