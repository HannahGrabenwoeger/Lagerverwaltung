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

        protected Task<string?> GetUserRoleAsync()
        {
            // Alle Claims ausgeben
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }

            // Rolle direkt aus JWT-Claim lesen
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            Console.WriteLine($"[JWT Claim] Rolle: {role}");

            return Task.FromResult(role);
        }
        
        [Authorize]
        [HttpGet("user-role")]
        public async Task<IActionResult> GetUserRole()
        {
            var uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized("Kein UID im Token gefunden.");
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