using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using Backend.Dtos;
using Backend.Interfaces;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthentificationController : ControllerBase
    {

        [HttpPost("verify-firebase-token")]
        public async Task<IActionResult> VerifyFirebaseToken(string idToken)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string uid = decodedToken.Uid;
                string email = decodedToken.Claims.TryGetValue("email", out var claim) ? claim?.ToString() ?? "N/A" : "N/A";
                return Ok(new { message = "Token valid", uid = uid, email = email });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromBody] FirebaseAuthDto model)
        {
            var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(model.IdToken);
            return Ok(new { uid = decoded.Uid });
        }

        public class TokenModel
        {
            public required string IdToken { get; set; }
        }

        [HttpPost("get-uid")]
public async Task<IActionResult> GetUid([FromBody] string idToken)
{
    try
    {
        var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
        
        string uid = decodedToken.Uid;

        return Ok(new { uid = uid });
    }
    catch (FirebaseAuthException ex)
    {
        return Unauthorized(new { message = "Invalid token", error = ex.Message });
    }
}
    }
}