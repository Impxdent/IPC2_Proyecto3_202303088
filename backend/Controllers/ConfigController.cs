using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;
using IPC2_Proyecto3_202303088.backend.Models;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace IPC2_Proyecto3_202303088.backend.Controllers
{
    [ApiController]
    [Route("/")] 
    public class ConfigController : ControllerBase
    {
        private readonly string ClientesPath = Path.Combine("DataStorage", "MasterClientes.xml");
        private readonly string BancosPath = Path.Combine("DataStorage", "MasterBancos.xml");
        private readonly string FacturasPath = Path.Combine("DataStorage", "MasterFacturas.xml");
        private readonly string PagosPath = Path.Combine("DataStorage", "MasterPagos.xml");

        [HttpPost("grabarConfiguracion")]
        public IActionResult CargarConfiguracion([FromBody] string xmlContent)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlContent)) return BadRequest("El contenido XML está vacío.");

                XmlSerializer serializer = new XmlSerializer(typeof(ConfigData));
                ConfigData data;

                using (StringReader reader = new StringReader(xmlContent))
                {
                    data = (ConfigData)serializer.Deserialize(reader);
                }

                var resClientes = ActualizarClientes(data.Clientes);
                var resBancos = ActualizarBancos(data.Bancos);

                return Ok(new { 
                    mensaje = "Configuración procesada exitosamente.",
                    clientesCreados = resClientes.creados,
                    clientesActualizados = resClientes.actualizados,
                    bancosCreados = resBancos.creados,
                    bancosActualizados = resBancos.actualizados
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al procesar configuración: " + ex.Message });
            }
        }

        [HttpPost("limpiarDatos")]
        public IActionResult LimpiarDatos()
        {
            try
            {
                if (System.IO.File.Exists(ClientesPath)) System.IO.File.Delete(ClientesPath);
                if (System.IO.File.Exists(BancosPath)) System.IO.File.Delete(BancosPath);
                if (System.IO.File.Exists(FacturasPath)) System.IO.File.Delete(FacturasPath);
                if (System.IO.File.Exists(PagosPath)) System.IO.File.Delete(PagosPath);

                return Ok(new { mensaje = "Todos los datos han sido eliminados correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al limpiar datos: " + ex.Message });
            }
        }

        private (int creados, int actualizados) ActualizarClientes(List<Cliente> nuevos)
        {
            int creados = 0;
            int actualizados = 0;

            if (nuevos == null) return (0, 0);
            List<Cliente> actuales = LeerXml<List<Cliente>>(ClientesPath) ?? new List<Cliente>();

            foreach (var n in nuevos)
            {
                var existente = actuales.FirstOrDefault(c => c.Nit == n.Nit);
                if (existente != null)
                {
                    existente.Nombre = n.Nombre; 
                    actualizados++;
                }
                else
                {
                    actuales.Add(n); 
                    creados++;
                }
            }

            GuardarXml(actuales, ClientesPath);
            return (creados, actualizados);
        }

        private (int creados, int actualizados) ActualizarBancos(List<Banco> nuevos)
        {
            int creados = 0;
            int actualizados = 0;

            if (nuevos == null) return (0, 0);
            List<Banco> actuales = LeerXml<List<Banco>>(BancosPath) ?? new List<Banco>();

            foreach (var n in nuevos)
            {
                var existente = actuales.FirstOrDefault(b => b.Codigo == n.Codigo);
                if (existente != null)
                {
                    existente.Nombre = n.Nombre;
                    actualizados++;
                }
                else
                {
                    actuales.Add(n);
                    creados++;
                }
            }

            GuardarXml(actuales, BancosPath);
            return (creados, actualizados);
        }

        private T LeerXml<T>(string path) where T : class
        {
            if (!System.IO.File.Exists(path)) return null;
            try {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    return (T)ser.Deserialize(fs);
                }
            } catch { return null; }
        }

        private void GuardarXml<T>(T data, string path)
        {
            string folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
            
            XmlSerializer ser = new XmlSerializer(typeof(T));
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                ser.Serialize(fs, data);
            }
        }
    }
}