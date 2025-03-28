using System.Threading.Tasks;
using FirebaseAdmin.Auth;

namespace Backend.Services.Firebase
{
    public class FirebaseAuthWrapper : IFirebaseAuthWrapper
    {
        public Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
        {
            return FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
        }
    }
}