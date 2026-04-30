using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace frontend.Pages.Consultas
{
    public class IngresosModel : PageModel
    {
        public List<ResumenBanco> DatosGrafica { get; set; } = new List<ResumenBanco>();

        [BindProperty(SupportsGet = true)]
        public int MesSeleccionado { get; set; } = 1; 

        [BindProperty(SupportsGet = true)]
        public int AnioSeleccionado { get; set; } = 2024; 

        public async Task OnGetAsync()
        {
            await CargarDatos();
        }

        public async Task OnPostAsync()
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
                using var client = new HttpClient(handler);

                string url = $"http://localhost:5142/devolverResumenPagos?mes={MesSeleccionado}&anio={AnioSeleccionado}";
                var response = await client.GetFromJsonAsync<List<ResumenBanco>>(url);
                
                if (response != null) DatosGrafica = response;
            }
            catch { /* Manejar error */ }
        }
    }

    public class ResumenBanco
    {
        public string Nombre { get; set; }
        public decimal Total { get; set; }
    }
}