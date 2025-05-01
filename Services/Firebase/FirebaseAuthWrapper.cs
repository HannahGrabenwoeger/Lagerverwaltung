using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace Backend.Services.Firebase
{
    public class FirebaseAuthWrapper : IFirebaseAuthWrapper
    {
        public async Task<string> VerifyIdTokenAndGetUidAsync(string idToken)
        {
            try
            {
                Console.WriteLine("Token wird überprüft...");
                var token = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                Console.WriteLine("Token erfolgreich verifiziert!");
                return token.Uid;
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine($"FirebaseAuthException: {ex.Message}");
                throw new UnauthorizedAccessException("Firebase token is invalid", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Allgemeiner Fehler in VerifyIdTokenAndGetUidAsync: {ex.Message}");
                throw;
            }
        }
    }
}