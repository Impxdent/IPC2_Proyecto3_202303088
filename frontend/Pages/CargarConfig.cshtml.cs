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
            if (archivoXml == null || archivoXml.Length == 0)
            {
                Mensaje = "Error: Por favor selecciona un archivo XML.";
                Error = true;
                return Page();
            }

            try
            {
                using (var reader = new StreamReader(archivoXml.OpenReadStream()))
                {
                    string contenido = await reader.ReadToEndAsync();
                }

                Mensaje = "¡Archivo cargado con éxito!";
                Error = false;
            }
            catch (System.Exception ex)
            {
                Mensaje = "Error al procesar: " + ex.Message;
                Error = true;
            }

            return Page();
        }
    }
}