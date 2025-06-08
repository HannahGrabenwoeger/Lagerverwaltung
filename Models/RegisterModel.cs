using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;

        [RegularExpression("^(Manager|Employee)$", ErrorMessage = "Role must be either 'Manager' or 'Employee'.")]
        public string Role { get; set; } = string.Empty;
    }
}