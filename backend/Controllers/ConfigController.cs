using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;
using IPC2_Proyecto3_202303088.backend.Models;

namespace IPC2_Proyecto3_202303088.backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly string FolderPath = "DataStorage";

        [HttpPost("cargar")]
        public IActionResult CargarConfiguracion([FromBody] string xmlContent)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ConfigData));
                ConfigData data;

                using (StringReader reader = new StringReader(xmlContent))
                {
                    data = (ConfigData)serializer.Deserialize(reader);
                }

                ProcesarClientes(data.Clientes);
                ProcesarBancos(data.Bancos);

                return Ok(new { mensaje = "Configuración procesada exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al procesar el XML: " + ex.Message });
            }
        }

        private void ProcesarClientes(List<Cliente> nuevosClientes)
        {
            // Aquí irá la lógica para leer el archivo maestro de Clientes
            // verificar si el NIT ya existe para actualizarlo, o agregarlo si es nuevo.
            //estructura por ahora
        }

        private void ProcesarBancos(List<Banco> nuevosBancos)
        {
            // Similar a clientes, pero para el archivo maestro de Bancos.
        }

        [HttpDelete("reset")]
        public IActionResult ResetearDatos()
        {
            try
            {
                if (Directory.Exists(FolderPath))
                {
                    DirectoryInfo di = new DirectoryInfo(FolderPath);
                    foreach (FileInfo file in di.GetFiles()) file.Delete();
                }
                return Ok(new { mensaje = "Sistema reseteado a estado inicial." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al resetear: " + ex.Message });
            }
        }
    }
}