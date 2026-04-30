using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Text.Json;

namespace frontend.Pages
{
    public class CargarTransacModel : PageModel
    {
        [BindProperty]
        public string Mensaje { get; set; }
        
        [BindProperty]
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

                var resultado = await response.Content.ReadFromJsonAsync<RespuestaAPI>();

                if (response.IsSuccessStatusCode)
                {
                    Mensaje = resultado?.mensaje ?? "Transacciones procesadas exitosamente.";
                    Error = false;
                }
                else
                {
                    Mensaje = "Error en la API: " + (resultado?.mensaje ?? "No se pudo procesar el archivo.");
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

    public class RespuestaAPI
    {
        public string mensaje { get; set; }
    }
}