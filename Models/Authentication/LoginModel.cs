﻿using System.ComponentModel.DataAnnotations;
using TenentManagement.Models.User;

namespace TenentManagement.Models.Authentication
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
