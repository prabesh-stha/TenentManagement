namespace TenentManagement.Models.Payment
{
    public class PaymentQRImageModel
    {
        public int Id { get; set; }
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string ImageType { get; set; } = string.Empty;
        public int PaymentMethodId { get; set; }
        public int OwnerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string Base64Image => $"data:{ImageType};base64,{Convert.ToBase64String(ImageData)}";
    }
}