using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IPC2_Proyecto3_202303088.frontend.Models;
using System.Net.Http.Json;

namespace frontend.Pages.Consultas
{
    public class EstadoCuentaModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EstadoCuentaModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<EstadoCuentaResponse> Resultados { get; set; } = new List<EstadoCuentaResponse>();

        public async Task OnGetAsync(string nit)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                
                string url = "http://localhost:5142/devolverEstadoCuenta";
                if (!string.IsNullOrEmpty(nit))
                {
                    url += $"/{nit}";
                }

                var response = await client.GetFromJsonAsync<List<EstadoCuentaResponse>>(url);

                if (response != null)
                {
                    Resultados = response;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con la API: " + ex.Message);
            }
        }
    }
}