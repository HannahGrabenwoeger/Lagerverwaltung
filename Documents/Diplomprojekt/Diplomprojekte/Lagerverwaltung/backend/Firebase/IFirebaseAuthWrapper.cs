using System.Threading.Tasks;
using FirebaseAdmin.Auth;

namespace Backend.Services.Firebase
{
    public interface IFirebaseAuthWrapper
    {
        Task<FirebaseToken> VerifyIdTokenAsync(string idToken);
    }
}