using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class UserRolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserRolesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("is-manager/{uid}")]
        public async Task<IActionResult> IsManager(string uid)
        {
            var roleEntry = await _context.UserRoles.FirstOrDefaultAsync(r => r.FirebaseUid == uid);
            if (roleEntry == null || roleEntry.Role != "Manager")
            {
                return Unauthorized();
            }

            return Ok(new { isManager = true });
        }
    }
}