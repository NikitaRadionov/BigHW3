using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using ApiGateway.DTOs;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api")]
    public class GatewayController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GatewayController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        private string OrdersServiceUrl => _configuration["Services:OrdersService"];
        private string PaymentsServiceUrl => _configuration["Services:PaymentsService"];

        private void CopyUserIdHeader(HttpRequest request, HttpRequestMessage message)
        {
            if (request.Headers.TryGetValue("X-User-Id", out var userId))
                message.Headers.Add("X-User-Id", userId.ToString());
        }

        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            var request = new HttpRequestMessage(HttpMethod.Post, $"{OrdersServiceUrl}/orders")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            CopyUserIdHeader(Request, request);
            return await SafeProxyRequest(request, "OrderService");
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{OrdersServiceUrl}/orders");
            CopyUserIdHeader(Request, request);
            return await SafeProxyRequest(request, "OrderService");
        }

        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{OrdersServiceUrl}/orders/{id}");
            CopyUserIdHeader(Request, request);
            return await SafeProxyRequest(request, "OrderService");
        }

        [HttpPost("accounts")]
        public async Task<IActionResult> CreateAccount()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{PaymentsServiceUrl}/accounts");
            CopyUserIdHeader(Request, request);
            return await SafeProxyRequest(request, "PaymentService");
        }

        [HttpPost("accounts/deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            var request = new HttpRequestMessage(HttpMethod.Post, $"{PaymentsServiceUrl}/accounts/deposit")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            CopyUserIdHeader(Request, request);
            return await SafeProxyRequest(request, "PaymentService");
        }

        [HttpGet("accounts/balance")]
        public async Task<IActionResult> GetBalance()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{PaymentsServiceUrl}/accounts/balance");
            CopyUserIdHeader(Request, request);
            return await SafeProxyRequest(request, "PaymentService");
        }

        private async Task<IActionResult> SafeProxyRequest(HttpRequestMessage request, string serviceName)
        {
            try
            {
                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"{serviceName} is currently unavailable.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal gateway error.");
            }
        }
    }
}