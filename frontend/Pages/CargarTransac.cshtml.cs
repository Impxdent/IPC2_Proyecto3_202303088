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

                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                using var client = new HttpClient(handler);
                var response = await client.PostAsJsonAsync("http://localhost:5142/grabarTransaccion", contenidoXml);

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