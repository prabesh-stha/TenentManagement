using System.ComponentModel.DataAnnotations;

namespace TenentManagement.Models.Property
{
    public class PropertyModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter the name of the property.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Property Name")]
        public string Name { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the address of the propery")]
        public string Address { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please enter the description of the property.")]

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Type { get; set; }
        public string TypeName { get; set; } = string.Empty;

        public IEnumerable<PropertyTypeModel> PropertyTypes { get; set; } = new List<PropertyTypeModel>();
        public PropertyImageModel? PropertyImage { get; set; }
    }
}
