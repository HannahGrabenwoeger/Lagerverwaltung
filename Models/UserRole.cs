using Google.Cloud.Firestore;

namespace Backend.Models
{
    [FirestoreData]
    public class UserRole
    {
        [FirestoreProperty]
        public Guid Id { get; set; } = Guid.NewGuid();

        [FirestoreProperty]
        public string FirebaseUid { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Role { get; set; } = "Employee";
    }
}