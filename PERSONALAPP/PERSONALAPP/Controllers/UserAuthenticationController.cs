using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace PERSONALAPP.Controllers
{
    public class UserAuthenticationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserAuthenticationController> _logger;

        public UserAuthenticationController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<UserAuthenticationController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult UserSignUp()
        {
            return View();
        }

        public IActionResult UserLogin()
        {
            return View();
        }

        /// <summary>
        /// Forwards registration JSON to the Ocelot API Gateway (single API entry point).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Register(CancellationToken cancellationToken)
        {
            var gatewayBase = _configuration["ApiGateway:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(gatewayBase))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "ApiGateway:BaseUrl is not configured in appsettings."
                });
            }

            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var json = await reader.ReadToEndAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
            {
                return BadRequest(new { success = false, message = "Request body is required." });
            }

            var targetUrl = $"{gatewayBase}/api/portfolio/register";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, targetUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            try
            {
                var client = _httpClientFactory.CreateClient("ApiGateway");
                using var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                return new ContentResult
                {
                    StatusCode = (int)response.StatusCode,
                    Content = responseBody,
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration proxy failed calling {TargetUrl}", targetUrl);
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    success = false,
                    message = "Unable to reach the API Gateway. Ensure the gateway and Portfolio Service are running."
                });
            }
        }
    }
}
