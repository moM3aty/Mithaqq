using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Mithaqq.ViewModels;

namespace Mithaqq.Services
{
    public class PaypalService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _baseUrl;

        public PaypalService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _clientId = _configuration["PayPal:ClientId"];
            _clientSecret = _configuration["PayPal:ClientSecret"];
            _baseUrl = _configuration["PayPal:BaseUrl"];
            _httpClient = httpClient;
        }

        public string GetClientId() => _clientId;

        private async Task<string> GetAccessTokenAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                System.Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}")));

            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };
            request.Content = new FormUrlEncodedContent(content);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<JsonElement>(responseString);
            return token.GetProperty("access_token").GetString();
        }

        public async Task<PaypalCreateOrderResponse> CreateOrderAsync(decimal amount, string currency)
        {
            var accessToken = await GetAccessTokenAsync();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v2/checkout/orders");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var orderRequest = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = currency,
                            value = amount.ToString("F2")
                        }
                    }
                }
            };

            request.Content = new StringContent(JsonSerializer.Serialize(orderRequest), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PaypalCreateOrderResponse>(responseString);
        }


        public async Task<PaypalOrderResponse> CaptureOrderAsync(string orderId)
        {
            var accessToken = await GetAccessTokenAsync();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v2/checkout/orders/{orderId}/capture");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PaypalOrderResponse>(responseString);
        }
    }
}
