using Backend.Models;

namespace Backend.Interfaces
{
    public interface IUserService
    {
        Task<UserRole?> GetUserByUidAsync(string firebaseUid);
        Task<bool> UserExistsAsync(string firebaseUid);
    }
}