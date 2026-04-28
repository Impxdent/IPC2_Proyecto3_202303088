using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace frontend.Pages
{
    public class CargarConfigModel : PageModel
    {
        [BindProperty]
        public string Mensaje { get; set; }
        
        [BindProperty]
        public bool Error { get; set; }

        public void OnGet()
        {
            //para cuando cargue la página
        }

        public async Task<IActionResult> OnPostAsync(IFormFile archivoXml)
        {
            if (archivoXml == null) return Page();

            try
            {
                using var reader = new StreamReader(archivoXml.OpenReadStream());
                string contenidoXml = await reader.ReadToEndAsync();

                using var client = new HttpClient();
                var response = await client.PostAsJsonAsync("https://localhost:5142/api/config/cargar", contenidoXml);

                if (response.IsSuccessStatusCode)
                {
                    Mensaje = "Archivo procesado y guardado en el servidor.";
                    Error = false;
                }
                else
                {
                    Mensaje = "La API rechazó el archivo.";
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