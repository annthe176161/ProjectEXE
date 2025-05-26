using Microsoft.Extensions.Caching.Memory;
using ProjectEXE.Services.Interfaces;

namespace ProjectEXE.Services.Implementations
{
    public class SessionService : ISessionService
    {
        private readonly IMemoryCache _cache;

        public SessionService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task StoreEmailVerificationAsync(string sessionId, string email)
        {
            var key = $"EmailVerify:{sessionId}";
            _cache.Set(key, email, TimeSpan.FromHours(1)); // Hết hạn sau 1 giờ
            return Task.CompletedTask;
        }

        public Task StorePasswordResetAsync(string sessionId, string email)
        {
            var key = $"PasswordReset:{sessionId}";
            _cache.Set(key, email, TimeSpan.FromHours(1)); // Hết hạn sau 1 giờ
            return Task.CompletedTask;
        }

        public Task<bool> ValidateAndGetEmailAsync(string sessionId, string email, string sessionType)
        {
            var key = $"{sessionType}:{sessionId}";
            var storedEmail = _cache.Get<string>(key);

            var isValid = !string.IsNullOrEmpty(storedEmail) &&
                         storedEmail.Equals(email, StringComparison.OrdinalIgnoreCase);

            return Task.FromResult(isValid);
        }

        public Task RemoveSessionAsync(string sessionId, string sessionType)
        {
            var key = $"{sessionType}:{sessionId}";
            _cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}

