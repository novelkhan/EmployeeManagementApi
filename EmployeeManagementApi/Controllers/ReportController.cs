using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace EmployeeManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ReportController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [AllowAnonymous] // টেস্টিংয়ের জন্য, প্রোডাকশনে অথেনটিকেশন যোগ করো
        [HttpGet("generate")]
        public async Task<IActionResult> GenerateReport(string reportName = "EmployeeListReport", string format = "PDF")
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue($"application/{format.ToLower()}"));
            client.DefaultRequestHeaders.Add("User-Agent", "ASP.NET Core");

            var handler = new HttpClientHandler
            {
                UseDefaultCredentials = true // Windows Authentication
            };
            client = new HttpClient(handler);

            var reportServerUrl = $"http://novel_laptop/ReportServer?/EmployeeReports/{reportName}&rs:Command=Render&rs:Format={format}";
            var response = await client.GetAsync(reportServerUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                return File(content, $"application/{format.ToLower()}", $"{reportName}.{format.ToLower()}");
            }

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [AllowAnonymous] // টেস্টিংয়ের জন্য
        [HttpGet("preview")]
        public async Task<IActionResult> PreviewReport(string reportName = "EmployeeListReport")
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pdf"));
            client.DefaultRequestHeaders.Add("User-Agent", "ASP.NET Core");

            var handler = new HttpClientHandler
            {
                UseDefaultCredentials = true // Windows Authentication
            };
            client = new HttpClient(handler);

            var reportServerUrl = $"http://novel_laptop/ReportServer?/EmployeeReports/{reportName}&rs:Command=Render&rs:Format=PDF";
            var response = await client.GetAsync(reportServerUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                return File(content, "application/pdf"); // প্রিভিউয়ের জন্য ডাউনলোড ফাইল নাম ছাড়া
            }

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }
}