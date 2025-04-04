using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using Backend.Dtos;
using Backend.Interfaces;
using Backend.Services.Firebase;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthentificationController : ControllerBase
    {
        private readonly IFirebaseAuthWrapper _firebaseAuth;

        public AuthentificationController(IFirebaseAuthWrapper firebaseAuth)
        {
            _firebaseAuth = firebaseAuth;
        }

        [HttpPost("verify-firebase-token")]
        public async Task<IActionResult> VerifyFirebaseToken(string idToken)
        {
            try
            {
                var uid = await _firebaseAuth.VerifyIdTokenAndGetUidAsync(idToken);
                return Ok(new { message = "Token valid", uid = uid });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromBody] FirebaseAuthDto model)
        {
            var uid = await _firebaseAuth.VerifyIdTokenAndGetUidAsync(model.IdToken);
            return Ok(new { uid = uid });
        }

        [HttpPost("get-uid")]
        public async Task<IActionResult> GetUid([FromBody] string idToken)
        {
            try
            {
                var uid = await _firebaseAuth.VerifyIdTokenAndGetUidAsync(idToken);
                return Ok(new { uid = uid });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }
    }
}