using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace frontend.Pages
{
    public class CargarConfigModel : PageModel
    {
        [BindProperty]
        public string Mensaje { get; set; }
        
        [BindProperty]
        public bool Error { get; set; }
        public int ClientesC { get; set; }
        public int ClientesA { get; set; }
        public int BancosC { get; set; }
        public int BancosA { get; set; }
        public bool MostrarTabla { get; set; } = false;

        public void OnGet()
        {
            MostrarTabla = false;
        }

        public async Task<IActionResult> OnPostAsync(IFormFile archivoXml)
        {
            if (archivoXml == null) 
            {
                Mensaje = "Por favor, seleccione un archivo.";
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
                
                var response = await client.PostAsJsonAsync("http://localhost:5142/grabarConfiguracion", contenidoXml);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaConfig>();
                    
                    if (resultado != null)
                    {
                        ClientesC = resultado.clientesCreados;
                        ClientesA = resultado.clientesActualizados;
                        BancosC = resultado.bancosCreados;
                        BancosA = resultado.bancosActualizados;
                        MostrarTabla = true;
                        
                        Mensaje = "Configuración procesada exitosamente";
                        Error = false;
                    }
                }
                else
                {
                    Mensaje = "Error, el servidor no pudo procesar el archivo XML.";
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

    public class RespuestaConfig
    {
        public int clientesCreados { get; set; }
        public int clientesActualizados { get; set; }
        public int bancosCreados { get; set; }
        public int bancosActualizados { get; set; }
        public string mensaje { get; set; }
    }
}