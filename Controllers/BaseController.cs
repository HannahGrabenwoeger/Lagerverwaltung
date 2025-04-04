using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly AppDbContext _dbContext;

        public BaseController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async Task<string?> GetUserRole()
        {
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(uid)) return null;

            var role = await _dbContext.UserRoles
                .Where(r => r.FirebaseUid == uid)
                .Select(r => r.Role)
                .FirstOrDefaultAsync();

            return role;
        }
    }
}