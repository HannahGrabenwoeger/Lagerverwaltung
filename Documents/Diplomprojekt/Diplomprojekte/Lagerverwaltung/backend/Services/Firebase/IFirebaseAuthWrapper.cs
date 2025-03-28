using System.Threading.Tasks;

namespace Backend.Services.Firebase
{
    public interface IFirebaseAuthWrapper
    {
        Task<string> VerifyIdTokenAndGetUidAsync(string idToken);
    }
}