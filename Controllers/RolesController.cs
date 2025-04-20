using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
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
            if (_settings.TestMode)
            {
                return "Manager";
            }

            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(uid))
                return null;

            return await _context.UserRoles
                .Where(r => r.FirebaseUid == uid)
                .Select(r => r.Role)
                .FirstOrDefaultAsync();
        }

        [HttpGet("user-role")]
        public async Task<IActionResult> GetUserRole()
        {
            var role = await GetUserRoleAsync();
            if (role == null)
                return NotFound("No role found.");

            return Ok(new { role = role, isManager = role == "Manager" });
        }
    }
}