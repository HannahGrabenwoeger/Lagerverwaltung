using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class UserQueryService : IUserQueryService
    {
        private readonly AppDbContext _context;

        public UserQueryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserRole?> FindUserAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return await _context.UserRoles
                .FirstOrDefaultAsync(u => 
                    string.Equals(u.FirebaseUid, username.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}