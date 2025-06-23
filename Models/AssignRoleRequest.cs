namespace Backend.Models
{
    public class AssignRoleRequest
    {
        public string FirebaseUid { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}