using Backend.Dto;
using Backend.Models;

namespace Backend.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(RegisterModel model);
        Task<ApplicationUser?> LoginAsync(LoginModel model);
    }
}