using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Services
{
    public interface IUserQueryService
    {
        Task<string?> GetUserRoleAsync();

        Task<UserRole?> FindUserAsync(string firebaseUid);
    }
}