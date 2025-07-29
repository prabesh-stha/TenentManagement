using System.ComponentModel.DataAnnotations;

namespace TenentManagement.Models.Payment
{
    public class PaymentQRImageModel
    {
        public int Id { get; set; }
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string ImageType { get; set; } = string.Empty;
        [Required(ErrorMessage ="Please select payment method")]
        public int PaymentMethodId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string Base64Image => $"data:{ImageType};base64,{Convert.ToBase64String(ImageData)}";
    }
}