﻿using TenentManagement.Models.User;

namespace TenentManagement.ViewModel
{
    public class RenterUnitDetailViewModel
    {
        public int Id { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string UnitDescription { get; set; } = string.Empty;
        public float RentAmount { get; set; } = 1000;
        public DateTime? RentStartDate { get; set; }
        public DateTime? RentEndDate { get; set; }
        public int RenterId { get; set; }
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyAddress { get; set; } = string.Empty;
        public double PropertyLatitude { get; set; }
        public double PropertyLongitude { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string PropertyDescription { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public string OwnerFirstName { get; set; } = string.Empty;
        public string OwnerLastName { get; set; } = string.Empty;
        public string OwnerPhoneNumber { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;

        public UserImageModel? UserImage { get; set; }
    }
}
