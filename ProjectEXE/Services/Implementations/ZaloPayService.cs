using ProjectEXE.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ProjectEXE.ViewModel.Zalo;

namespace ProjectEXE.Services.Implementations
{
    public class ZaloPayService : IZaloPayService
    {
        private readonly IConfiguration _configuration;
        private readonly string _appId;
        private readonly string _key1;
        private readonly string _endpoint;

        public ZaloPayService(IConfiguration configuration)
        {
            _configuration = configuration;
            _appId = _configuration["ZaloPay:AppId"];
            _key1 = _configuration["ZaloPay:Key1"];
            _endpoint = _configuration["ZaloPay:CreateOrderEndpoint"];
        }

        public async Task<ZaloPayOrderResponse> CreateOrderAsync(string userId, string packageId, long amount)
        {
            var embedData = new { user_id = userId, package_id = packageId };
            var orderId = Guid.NewGuid().ToString();
            var data = new Dictionary<string, string>
        {
            { "appid", _appId },
            { "appuser", userId },
            { "apptime", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() },
            { "apptransid", DateTime.Now.ToString("yyMMdd") + "_" + orderId },
            { "item", "[]" },
            { "embeddata", JsonSerializer.Serialize(embedData) },
            { "amount", amount.ToString() },
            { "description", "Thanh toán gói dịch vụ" }
        };

            string dataStr = $"{_appId}|{data["apptransid"]}|{userId}|{amount}|{data["apptime"]}|{data["embeddata"]}|[]";
            data["mac"] = GenerateHmacSha256(dataStr, _key1);

            var client = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_endpoint, content);

            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ZaloPayOrderResponse>(result);
        }

        public bool ValidateCallback(ZaloPayCallbackData data)
        {
            var dataStr = $"{data.AppId}|{data.AppTransId}|{data.ZpTransId}|{data.Amount}|{data.ServerTime}";
            var mac = GenerateHmacSha256(dataStr, _key1);
            return mac == data.Mac;
        }

        private string GenerateHmacSha256(string input, string key)
        {
            var encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(key);
            using var hmacsha256 = new HMACSHA256(keyByte);
            byte[] messageBytes = encoding.GetBytes(input);
            return BitConverter.ToString(hmacsha256.ComputeHash(messageBytes)).Replace("-", "").ToLower();
        }
    }
}
