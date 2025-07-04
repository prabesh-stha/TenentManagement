using TenentManagement.Models.Property;
using TenentManagement.Models.Property.Unit;

namespace TenentManagement.ViewModel
{
    public class PropertyDetailViewModel
    {
        public PropertyModel Property { get; set; }
        public List<UnitModel> Units { get; set; } = new();
    }
}
