using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Backend.Models;
using Backend.Dtos;

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

            Console.WriteLine("UID aus JWT: " + User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return role;
        }

        [Authorize]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            var currentRole = await GetUserRoleAsync();
            if (currentRole != "manager")
                return Forbid("Nur Manager dürfen Rollen zuweisen.");

            if (string.IsNullOrWhiteSpace(request.FirebaseUid) || string.IsNullOrWhiteSpace(request.Role))
                return BadRequest("FirebaseUid und Role müssen angegeben werden.");

            var existing = await _context.UserRoles
                .FirstOrDefaultAsync(r => r.FirebaseUid == request.FirebaseUid);

            if (existing != null)
            {
                existing.Role = request.Role;
            }
            else
            {
                _context.UserRoles.Add(new UserRole
                {
                    FirebaseUid = request.FirebaseUid,
                    Role = request.Role
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Rolle '{request.Role}' wurde für UID '{request.FirebaseUid}' gesetzt." });
        }

        
        [Authorize]
        [HttpGet("user-role")]
        public async Task<IActionResult> GetUserRole()
        {
            var uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(uid))
                return Unauthorized("No UID found in token.");

            if (!await _context.UserRoles.AnyAsync())
            {
                var firstRole = new UserRole
                {
                    FirebaseUid = uid,
                    Role = "manager"
                };

                _context.UserRoles.Add(firstRole);
                await _context.SaveChangesAsync();

                return Ok(new { role = "manager" });
            }

            var role = await _context.UserRoles
                .Where(r => r.FirebaseUid == uid)
                .Select(r => r.Role)
                .FirstOrDefaultAsync();

            if (role == null)
                return Ok(new { role = "none" });
            return Ok(new { role });
        }
    }
}