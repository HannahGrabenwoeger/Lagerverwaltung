using System.Security.Claims;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Services;
using Backend.Models;

namespace Backend.Services
{
    public class UserQueryService : IUserQueryService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserQueryService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserRole?> FindUserAsync(string firebaseUid)
        {
            var trimmedLower = firebaseUid.Trim().ToLower();
            var roles = await _context.UserRoles
                .AsNoTracking()
                .ToListAsync();

            return roles.FirstOrDefault(u => u.FirebaseUid.Trim().ToLower() == trimmedLower);
        }

        public async Task<string?> GetUserRoleAsync()
        {
            var uid = _httpContextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value;

            Console.WriteLine("UID aus Token: " + uid);

            if (string.IsNullOrEmpty(uid))
                return null;

            var allRoles = await _context.UserRoles.AsNoTracking().ToListAsync();

            foreach (var role in allRoles)
            {
                Console.WriteLine($"DB-Eintrag: {role.FirebaseUid} → {role.Role}");
            }

            var matched = allRoles.FirstOrDefault(r =>
                r.FirebaseUid.Trim().ToLower() == uid.Trim().ToLower());

            return matched?.Role;
        }
    }
}