using System;

namespace Backend.Models
{
    public class ApplicationUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirebaseUid { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee";
    }
}