using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Services
{
    public interface IUserQueryService
    {
        Task<UserRole?> FindUserAsync(string firebaseUid);
    }
}