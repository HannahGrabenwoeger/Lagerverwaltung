using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos
{
    public class UserDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}