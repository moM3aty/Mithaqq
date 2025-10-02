using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Mithaqq.Services
{
    public class BunnyService
    {
        private readonly string _securityKey;
        private readonly string _pullZoneUrl;

        public BunnyService(IConfiguration configuration)
        {
            _securityKey = configuration["BunnyNet:SecurityKey"];
            _pullZoneUrl = configuration["BunnyNet:PullZoneUrl"];
        }

        public string GenerateSecureUrl(string videoId, long? libraryId)
        {
            // Check if we should use secure token authentication.
            // It's disabled if the key is null, empty, or still the placeholder text.
            bool useTokenAuth = !string.IsNullOrEmpty(_securityKey) && !_securityKey.StartsWith("ضع-مفتاح");

            // Essential information must be present to generate any URL.
            if (!libraryId.HasValue || string.IsNullOrEmpty(videoId) || string.IsNullOrEmpty(_pullZoneUrl) || _pullZoneUrl.StartsWith("your-pullzone"))
            {
                // Cannot generate URL without essential IDs or a valid Pull Zone URL.
                return "";
            }

            string path = $"/hls/{libraryId.Value}/{videoId}/playlist.m3u8";

            if (useTokenAuth)
            {
                // --- REAL SECURE URL LOGIC ---
                long expiration = DateTimeOffset.UtcNow.AddHours(3).ToUnixTimeSeconds();
                string hashable = _securityKey + path + expiration;

                using (var sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashable));
                    string token = Convert.ToBase64String(bytes)
                        .Replace("\n", "")
                        .Replace("=", "")
                        .Replace("/", "_")
                        .Replace("+", "-");

                    return $"https://{_pullZoneUrl}{path}?token={token}&expires={expiration}";
                }
            }
            else
            {
                // --- INSECURE, DIRECT URL LOGIC (Used when no Security Key is provided) ---
                return $"https://{_pullZoneUrl}{path}";
            }
        }
    }
}

