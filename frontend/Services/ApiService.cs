using System.Net.Http;
using System.Threading.Tasks;

namespace IPC2_Proyecto3_202303088.frontend.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:5142/api"; 

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> ResetDatos()
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/reset");
            return await response.Content.ReadAsStringAsync();
        }
        
    }
}