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
                Console.WriteLine("Token valid. UID: " + token.Uid);
                return token.Uid;
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine("FirebaseAuthException: " + ex.Message);
                throw new UnauthorizedAccessException("Invalid Firebase token", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("General token verification error: " + ex.Message);
                throw;
            }
        }
    }
}