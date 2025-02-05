using Microsoft.AspNetCore.Identity;
using System;

namespace Backend.Models
{
        public class ApplicationUser : IdentityUser<Guid>
    {
        public string? FullName { get; set; } 
    }
}