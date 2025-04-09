using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        protected readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        protected async Task<string?> GetUserRoleAsync()
        {
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(uid))
            {
                return null;
            }

            var role = await _context.UserRoles
                .Where(r => r.FirebaseUid == uid)
                .Select(r => r.Role)
                .FirstOrDefaultAsync();

            return role;
        }

        [HttpGet("user-role")]
        public async Task<IActionResult> GetUserRole()
        {
            var role = await GetUserRoleAsync();
            if (role == null)
            {
                return NotFound("Keine Rolle gefunden.");
            }
            return Ok(new { role = role, isManager = role == "Manager" });
        }

        [HttpGet("is-manager/{uid}")]
        public async Task<IActionResult> IsManager(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                return BadRequest("uid darf nicht leer sein.");
            }

            var role = await _context.UserRoles
                .Where(r => r.FirebaseUid == uid)
                .Select(r => r.Role)
                .FirstOrDefaultAsync();

            if (role != "Manager")
            {
                return Unauthorized();
            }

            return Ok(new { isManager = true });
        }
    }
}