using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace frontend.Pages.Consultas
{
    public class IngresosModel : PageModel
    {
        public List<IngresoView> ReporteIngresos { get; set; } = new List<IngresoView>();

        public async Task OnGetAsync()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetFromJsonAsync<List<IngresoView>>("https://localhost:5142/api/transacciones/ingresos-mensuales");
                if (response != null)
                {
                    ReporteIngresos = response;
                }
            }
            catch { /* Manejar error */ }
        }
    }

    public class IngresoView
    {
        public string MesAnio { get; set; }
        public decimal Total { get; set; }
    }
}