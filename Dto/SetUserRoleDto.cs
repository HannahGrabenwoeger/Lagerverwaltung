 namespace Backend.Dtos
{
    public class SetUserRoleDto
    {
        public string FirebaseUid { get; set; } = string.Empty;
        public string Role { get; set; } = "employee";
    }
}