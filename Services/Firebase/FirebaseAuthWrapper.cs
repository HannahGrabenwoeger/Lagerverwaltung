using FirebaseAdmin.Auth;
using System;
using System.Threading.Tasks;

namespace Backend.Services.Firebase
{
    public class FirebaseAuthWrapper : IFirebaseAuthWrapper
    {
        public async Task<string> VerifyIdTokenAndGetUidAsync(string idToken)
        {
            try
            {
                Console.WriteLine("🔐 Verifiziere Firebase Token...");
                var token = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                Console.WriteLine("✅ Token gültig. UID: " + token.Uid);
                return token.Uid;
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine("❌ FirebaseAuthException: " + ex.Message);
                throw new UnauthorizedAccessException("Ungültiger Firebase-Token", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Allgemeiner Fehler bei der Tokenverifizierung: " + ex.Message);
                throw;
            }
        }
    }
}