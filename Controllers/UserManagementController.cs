using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FirebaseAdmin.Auth;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user-management")]
    public class UserManagementController : ControllerBase
    {
        [HttpGet("uid-by-email/{email}")]
        public async Task<IActionResult> GetUidByEmail(string email)
        {
            try
            {
                var user = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
                return Ok(new { uid = user.Uid });
            }
            catch (FirebaseAuthException ex)
            {
                return NotFound(new { message = "User not found", error = ex.Message });
            }
        }
    }
}