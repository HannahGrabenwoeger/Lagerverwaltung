using FirebaseAdmin.Auth;

namespace Backend.Services.Firebase
{
    public class FirebaseAuthWrapper : IFirebaseAuthWrapper
    {
        public async Task<string> VerifyIdTokenAndGetUidAsync(string idToken)
        {
            try
            {
                var token = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                return token.Uid;
            }
            catch (FirebaseAuthException ex)
            {
                throw new UnauthorizedAccessException("Firebase token is invalid", ex);
            }
        }
    }
}