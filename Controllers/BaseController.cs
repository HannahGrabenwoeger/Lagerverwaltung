using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController : ControllerBase
    {
        protected readonly AppDbContext _dbContext;

        public BaseController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Hinzufügen der Route, um diese Methode über einen HTTP GET Aufruf zu erreichen
        [HttpGet("user-role")]
        public async Task<string?> GetUserRole()
        {
            var user = User;
            if (user == null)
            {
                // Hier könntest du Log-Ausgaben oder eine Exception werfen, je nach Bedarf
                return null;
            }

            var uid = user.FindFirst("user_id")?.Value ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(uid))
            {
                // Hier auch Log-Ausgaben hinzufügen, wenn nötig
                return null;
            }

            var role = await _dbContext.UserRoles
                .Where(r => r.FirebaseUid == uid)
                .Select(r => r.Role)
                .FirstOrDefaultAsync();

            return role;
        }
    }
}