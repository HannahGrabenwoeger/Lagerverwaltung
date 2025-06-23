using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/audit-logs")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogsController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<string?> GetUserRoleAsync()
        {
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(uid)) return null;

            var role = await _context.UserRoles
                .Where(r => r.FirebaseUid == uid)
                .Select(r => r.Role)
                .FirstOrDefaultAsync();

            return role;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs()
        {
            var role = await GetUserRoleAsync();
            if (!string.Equals(role, "manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var logs = await _context.AuditLogs.ToListAsync();
            return Ok(logs);
        }
    }
}