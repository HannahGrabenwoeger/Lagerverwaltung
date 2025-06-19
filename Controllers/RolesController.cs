using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        protected readonly AppDbContext _context;
        private readonly AppSettings _settings;

        public RolesController(AppDbContext context, AppSettings settings)
        {
            _context = context;
            _settings = settings;
        }

        protected async Task<string?> GetUserRoleAsync()
        {
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(uid)) return null;

            var role = await _context.UserRoles
                .Where(r => r.FirebaseUid == uid)
                .Select(r => r.Role)
                .FirstOrDefaultAsync();

            Console.WriteLine($"[DB-Rolle] UID: {uid}, Rolle: {role}");
            return role;
        }
        
        [Authorize]
        [HttpGet("user-role")]
        public async Task<IActionResult> GetUserRole()
        {
            var uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized("No UID found in token.");
            }

            var role = await _context.UserRoles
                .Where(r => r.FirebaseUid == uid)
                .Select(r => r.Role)
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return NotFound("No role found.");
            }

            return Ok(new { role });
        }
    }
}