using FirebaseAdmin.Auth;
using System.Threading.Tasks;

namespace Backend.Services.Firebase
{
    public class FirebaseAuthWrapper : IFirebaseAuthWrapper
    {
        public async Task<string> VerifyIdTokenAndGetUidAsync(string idToken)
        {
            var token = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            return token.Uid;
        }
    }
}