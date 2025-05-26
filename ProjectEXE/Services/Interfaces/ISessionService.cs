

namespace ProjectEXE.Services.Interfaces
{
    public  interface ISessionService
    {
       
        Task StoreEmailVerificationAsync(string sessionId, string email);
        Task StorePasswordResetAsync(string sessionId, string email);
        Task<bool> ValidateAndGetEmailAsync(string sessionId, string email, string sessionType);
        Task RemoveSessionAsync(string sessionId, string sessionType);


    }
}
