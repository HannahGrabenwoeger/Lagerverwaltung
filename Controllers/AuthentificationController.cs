using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FirebaseAdmin.Auth;
using Backend.Dtos;
using Backend.Services.Firebase;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthentificationController : ControllerBase
    {
        private readonly IFirebaseAuthWrapper _firebaseAuth;
        private readonly IUserQueryService _userQueryService;

        public AuthentificationController(IFirebaseAuthWrapper firebaseAuth, IUserQueryService userQueryService)
        {
            _firebaseAuth = firebaseAuth;
            _userQueryService = userQueryService;
        }

        [HttpPost("verify-firebase-token")]
        public async Task<IActionResult> VerifyFirebaseToken([FromBody] FirebaseTokenRequest request)
        {
            Console.WriteLine("Request received!");

            if (string.IsNullOrEmpty(request.IdToken))
            {
                Console.WriteLine("No token passed.");
                return BadRequest(new { message = "Token missing in request." });
            }

            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);

                // UID: entweder direkt oder aus sub
                var uid = decodedToken.Uid ?? decodedToken.Claims.GetValueOrDefault("sub")?.ToString();

                if (string.IsNullOrEmpty(uid))
                {
                    Console.WriteLine("No UID or sub claim found.");
                    return Unauthorized(new { message = "Kein UID im Token gefunden." });
                }

                // Rolle aus Custom Claims holen
                var role = decodedToken.Claims.TryGetValue("role", out var roleObj) ? roleObj?.ToString() : "none";

                Console.WriteLine($"UID: {uid}");
                Console.WriteLine($"Role: {role}");

                return Ok(new
                {
                    uid,
                    role
                });
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine($"FirebaseAuthException: {ex.Message}");
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }


        [HttpGet("secure-data")]
        [Authorize]
        public async Task<IActionResult> GetSecureData()
        {
            var uid = User.FindFirst("user_id")?.Value;
            var role = await _userQueryService.GetUserRoleAsync();

            Console.WriteLine($"[SECURE-DATA] UID: {uid}");
            Console.WriteLine($"[SECURE-DATA] Rolle laut Service: {role}");

            return Ok(new { message = $"Hello , UID: {uid}, Role: {role ?? "none"}" });
        }
    }
}