using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // Endpunkt, um die Rolle des aktuell authentifizierten Benutzers abzurufen
        // Beispiel-Aufruf: GET /api/roles/user-role
        [HttpGet("user-role")]
        public async Task<IActionResult> GetUserRole()
        {
            // Extrahiere die Firebase-ID aus den Claims des authentifizierten Benutzers
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized("Keine Benutzer-ID gefunden.");
            }

            var role = await _context.UserRoles
                .Where(r => r.FirebaseUid == uid)
                .Select(r => r.Role)
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return NotFound("Keine Rolle gefunden.");
            }

            // Kombinierte Antwort: Rolle als String und ein boolescher Wert, ob es ein Manager ist
            return Ok(new { role = role, isManager = role == "Manager" });
        }

        // Endpunkt, um zu prüfen, ob der Benutzer mit der angegebenen Firebase-ID Manager ist
        // Beispiel-Aufruf: GET /api/roles/is-manager/{uid}
        [HttpGet("is-manager/{uid}")]
        public async Task<IActionResult> IsManager(string uid)
        {
            if (string.IsNullOrEmpty(uid))
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