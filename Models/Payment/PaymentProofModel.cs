namespace TenentManagement.Models.Payment
{
    public class PaymentProofModel
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string ImageType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int PaymentMethodId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public string Base64Image => $"data:{ImageType};base64,{Convert.ToBase64String(ImageData)}";

    }
}
