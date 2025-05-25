using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ProjectEXE.Services.TokenStorage
{
    public class TokenStore
    {
        // Concurrent dictionary to safely store tokens across requests
        private static readonly ConcurrentDictionary<string, TokenInfo> _tokens = new();
        private static readonly ConcurrentDictionary<string, bool> _verifiedEmails = new();

        public static Task StoreTokenAsync(string email, string token, string tokenType)
        {
            _tokens[GetKey(email, tokenType)] = new TokenInfo
            {
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            return Task.CompletedTask;
        }

        public static Task<bool> ValidateTokenAsync(string email, string token, string tokenType)
        {
            string key = GetKey(email, tokenType);

            if (_tokens.TryGetValue(key, out TokenInfo tokenInfo) &&
                tokenInfo.Token == token &&
                tokenInfo.ExpiresAt > DateTime.UtcNow)
            {
                // KHÔNG XÓA TOKEN Ở ĐÂY - chỉ validate
                // Token sẽ được xóa sau khi sử dụng thành công
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public static Task MarkEmailAsVerifiedAsync(string email)
        {
            _verifiedEmails[email] = true;

            // Remove the verification token as it's no longer needed
            _tokens.TryRemove(GetKey(email, "EmailVerification"), out _);

            return Task.CompletedTask;
        }

        public static Task<bool> IsEmailVerifiedAsync(string email)
        {
            return Task.FromResult(_verifiedEmails.TryGetValue(email, out bool verified) && verified);
        }

        private static string GetKey(string email, string tokenType)
        {
            return $"{email}:{tokenType}";
        }
        public static Task RemoveTokenAsync(string email, string tokenType)
        {
            string key = GetKey(email, tokenType);
            _tokens.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        private class TokenInfo
        {
            public string Token { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}