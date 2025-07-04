using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Xml;

namespace TenentManagement.Models.Property.Unit
{
    public class UnitModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter the name of the unit.")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please enter the address of the unit.")]
        public string Description { get; set; } = string.Empty;
        public bool IsVacant { get; set; } = true;
        [Required(ErrorMessage = "Please enter the rent amount for the unit.")]
        public float RentAmount { get; set; }
        public DateTime RentStartDate { get; set; } = DateTime.Now;
        public DateTime RentEndDate { get; set; } = DateTime.Now.AddMonths(1);
        public int? RenterId { get; set; } = null;
        public int PropertyId { get; set; }
    }
}
