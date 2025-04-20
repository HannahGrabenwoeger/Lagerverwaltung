using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Services;

namespace Backend.Servicesxs
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
            var trimmedLower = username.Trim().ToLower();
            var roles = await _context.UserRoles
                .AsNoTracking()
                .ToListAsync();

            return roles.FirstOrDefault(u =>
                u.FirebaseUid.Trim().ToLower() == trimmedLower
            );
        }
    }
}