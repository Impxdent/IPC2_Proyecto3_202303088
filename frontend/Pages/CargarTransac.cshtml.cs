using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace frontend.Pages
{
    public class CargarTransacModel : PageModel
    {
        [BindProperty]
        public string Mensaje { get; set; }
        public bool Error { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(IFormFile archivoXml)
        {
            if (archivoXml == null || archivoXml.Length == 0)
            {
                Mensaje = "Error: Seleccione un archivo transac.xml válido.";
                Error = true;
                return Page();
            }

            try
            {
                using var reader = new StreamReader(archivoXml.OpenReadStream());
                string contenidoXml = await reader.ReadToEndAsync();

                using var client = new HttpClient();
                var response = await client.PostAsJsonAsync("https://localhost:5142/api/transacciones/cargar", contenidoXml);

                if (response.IsSuccessStatusCode)
                {
                    Mensaje = "Transacciones procesadas y saldos actualizados exitosamente.";
                    Error = false;
                }
                else
                {
                    var errorData = await response.Content.ReadFromJsonAsync<dynamic>();
                    Mensaje = "Error en la API: " + errorData?.mensaje;
                    Error = true;
                }
            }
            catch (Exception ex)
            {
                Mensaje = "Error de conexión: " + ex.Message;
                Error = true;
            }

            return Page();
        }
    }
}