using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace Backend.Services.Firebase
{
    public class FirebaseAuthWrapper : IFirebaseAuthWrapper
    {
        static FirebaseAuthWrapper()
        {
            // Nur initialisieren, wenn noch kein FirebaseApp existiert
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("Secrets/service-account.json")
                });
            }
        }

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