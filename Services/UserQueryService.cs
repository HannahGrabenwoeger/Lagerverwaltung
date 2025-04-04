using Backend.Models;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class UserQueryService : IUserQueryService
    {
        public UserQueryService() { }

        public async Task<UserRole?> FindUserAsync(string firebaseUid)
        {
            return await GetUserRoleByUid(firebaseUid);
        }

        private async Task<UserRole?> GetUserRoleByUid(string firebaseUid)
        {
            await Task.Delay(50);
            return new UserRole
            {
                FirebaseUid = firebaseUid,
                Role = "Employee"
            };
        }
    }
}