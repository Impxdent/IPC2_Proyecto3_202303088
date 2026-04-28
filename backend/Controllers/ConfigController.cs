using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;
using IPC2_Proyecto3_202303088.backend.Models;
using System.IO;

namespace IPC2_Proyecto3_202303088.backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly string ClientesPath = Path.Combine("DataStorage", "MasterClientes.xml");
        private readonly string BancosPath = Path.Combine("DataStorage", "MasterBancos.xml");

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

                ActualizarClientes(data.Clientes);
                ActualizarBancos(data.Bancos);

                return Ok(new { mensaje = "Datos actualizados correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error: " + ex.Message });
            }
        }

        private void ActualizarClientes(List<Cliente> nuevos)
        {
            List<Cliente> actuales = LeerXml<List<Cliente>>(ClientesPath) ?? new List<Cliente>();

            foreach (var n in nuevos)
            {
                var existente = actuales.FirstOrDefault(c => c.Nit == n.Nit);
                if (existente != null)
                    existente.Nombre = n.Nombre; 
                else
                    actuales.Add(n); 
            }

            GuardarXml(actuales, ClientesPath);
        }

        private void ActualizarBancos(List<Banco> nuevos)
        {
            List<Banco> actuales = LeerXml<List<Banco>>(BancosPath) ?? new List<Banco>();

            foreach (var n in nuevos)
            {
                var existente = actuales.FirstOrDefault(b => b.Codigo == n.Codigo);
                if (existente != null)
                    existente.Nombre = n.Nombre;
                else
                    actuales.Add(n);
            }

            GuardarXml(actuales, BancosPath);
        }

        //manejo de archivos xml 
        private T LeerXml<T>(string path) where T : class
        {
            if (!System.IO.File.Exists(path)) return null;
            XmlSerializer ser = new XmlSerializer(typeof(T));
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                return (T)ser.Deserialize(fs);
            }
        }

        private void GuardarXml<T>(T data, string path)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                ser.Serialize(fs, data);
            }
        }
    }
}