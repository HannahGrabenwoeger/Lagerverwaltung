using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FirebaseAdmin.Auth;
using Backend.Dtos;
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
        public async Task<IActionResult> VerifyFirebaseToken([FromBody] FirebaseTokenRequest request)
        {
            Console.WriteLine("Anfrage erhalten!");

            if (!string.IsNullOrEmpty(request.IdToken))
            {
                var tokenPreview = request.IdToken.Length > 32
                    ? request.IdToken.Substring(0, 32) + "..."
                    : request.IdToken;

                Console.WriteLine($"Token: {tokenPreview}");
            }
            else
            {
                Console.WriteLine("Kein Token übergeben.");
                return BadRequest(new { message = "Token fehlt im Request." });
            }

            try
            {
                var uid = await _firebaseAuth.VerifyIdTokenAndGetUidAsync(request.IdToken);
                Console.WriteLine($"UID: {uid}");
                return Ok(new TokenResponseDto { Uid = uid });
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine($"FirebaseAuthException: {ex.Message}");
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Allgemeiner Fehler: {ex.Message}");
                return new UnauthorizedObjectResult(new { message = "Token ungültig", error = ex.Message });
            }
        }

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromBody] FirebaseAuthDto model)
        {
            var uid = await _firebaseAuth.VerifyIdTokenAndGetUidAsync(model.IdToken);
            return Ok(new TokenResponseDto { Uid = uid });
        }

        [HttpGet("secure-data")]
        [Authorize]
        public IActionResult GetSecureData()
        {
            var email = User.FindFirst("email")?.Value;
            var uid = User.FindFirst("user_id")?.Value;
            var role = User.FindFirst("role")?.Value ?? "none";

            return Ok(new
            {
                message = $"Hallo {email}, UID: {uid}, Rolle: {role}"
            });
        }
    }
}