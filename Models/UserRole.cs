namespace Backend.Models
{
    public class UserRole
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirebaseUid { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee";
    }
}