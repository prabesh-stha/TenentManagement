using System.ComponentModel.DataAnnotations;
using TenentManagement.Services.Property.UtilityBill;

namespace TenentManagement.Models.Property.Utility
{
    public class UtilityBillModel: IValidatableObject
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Please select valid utility type")]
        public int? UtilityTypeId { get; set; }
        public string UtilityType { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please select property")]

        public int? PropertyId { get; set; }

        public string PropertyName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime Month { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? TotalUnit { get; set; }
        public int? ConsumedUnit { get; set; }
        [Required(ErrorMessage = "Please enter amount")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public float? Amount { get; set; }
        public bool IsPaid { get; set; } = false;
        public IEnumerable<UtilityTypeModel> UtilityTypes { get; set; } = new List<UtilityTypeModel>();
        public UtilityBillImageModel? UtilityBillImage { get; set; } = null;
        public IEnumerable<OwnerPropertiesNameModel> OwnerPropertyName { get; set; } = new List<OwnerPropertiesNameModel>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!UtilityTypeId.HasValue)
            {
                yield return new ValidationResult("Please select a valid utility type.", new[] { nameof(UtilityTypeId) });
            }

            if (UtilityTypeId == 1 || UtilityTypeId == 2) // Electricity or Water
            {

                if (!ConsumedUnit.HasValue)
                {
                    yield return new ValidationResult("Please enter consumed units.", new[] { nameof(ConsumedUnit) });
                }
                if(ConsumedUnit <= 0)
                {
                    yield return new ValidationResult("Unit must be greater than 0.", new[] { nameof(ConsumedUnit) });
                }
                if(TotalUnit <= 0)
                {
                    yield return new ValidationResult("Unit must be greater than 0.", new[] { nameof(TotalUnit) });
                }
            }

            if (Amount <= 0)
            {
                yield return new ValidationResult("Amount must be greater than 0.", new[] { nameof(Amount) });
            }
        }

    }
}