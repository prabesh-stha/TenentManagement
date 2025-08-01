using Microsoft.AspNetCore.Mvc.Rendering;
using TenentManagement.Models.Property;

namespace TenentManagement.ViewModel
{
    public class PropertySelectionViewModel
    {
        public int? SelectedPropertyId { get; set; }
        public List<OwnerPropertiesNameModel> PropertyList { get; set; } = new List<OwnerPropertiesNameModel>();

        public int? SelectedUnitId { get; set; }

    }
}
