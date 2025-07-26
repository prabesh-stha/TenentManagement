namespace TenentManagement.Models.User
{
    public class UserImageModel
    {
        public int Id { get; set; }
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string ImageType { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Base64Image => $"data:{ImageType};base64,{Convert.ToBase64String(ImageData)}";


    }
}
