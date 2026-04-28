using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;
using IPC2_Proyecto3_202303088.backend.Models;
using System.Globalization;

namespace IPC2_Proyecto3_202303088.backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransaccionesController : ControllerBase
    {
        private readonly string FacturasPath = Path.Combine("DataStorage", "MasterFacturas.xml");
        private readonly string ClientesPath = Path.Combine("DataStorage", "MasterClientes.xml");

        [HttpPost("cargar")]
        public IActionResult CargarTransacciones([FromBody] string xmlContent)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TransaccionesData));
                TransaccionesData data;
                using (StringReader reader = new StringReader(xmlContent))
                {
                    data = (TransaccionesData)serializer.Deserialize(reader);
                }

                ProcesarFacturas(data.Facturas);

                foreach (var pago in data.Pagos)
                {
                    AplicarPagoFIFO(pago);
                }

                return Ok(new { mensaje = "Transacciones procesadas y pagos aplicados." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error: " + ex.Message });
            }
        }

        private void ProcesarFacturas(List<FacturaXml> nuevas)
        {
            var actuales = LeerXml<List<FacturaXml>>(FacturasPath) ?? new List<FacturaXml>();
            foreach (var f in nuevas)
            {
                if (!actuales.Any(x => x.Numero == f.Numero))
                {
                    f.SaldoPendiente = f.Monto; 
                    f.Estado = "Pendiente";
                    actuales.Add(f);
                }
            }
            GuardarXml(actuales, FacturasPath);
        }

        private void AplicarPagoFIFO(PagoXml pago)
        {
            var todasFacturas = LeerXml<List<FacturaXml>>(FacturasPath) ?? new List<FacturaXml>();
            var clientes = LeerXml<List<Cliente>>(ClientesPath) ?? new List<Cliente>();
            
            var cliente = clientes.FirstOrDefault(c => c.Nit == pago.Nit);
            if (cliente == null) return; 

            var pendientes = todasFacturas
                .Where(f => f.Nit == pago.Nit && f.Estado != "Pagada")
                .OrderBy(f => DateTime.ParseExact(f.Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                .ToList();

            decimal montoDisponible = pago.Monto;

            foreach (var factura in pendientes)
            {
                if (montoDisponible <= 0) break;

                if (montoDisponible >= factura.SaldoPendiente)
                {
                    montoDisponible -= factura.SaldoPendiente;
                    factura.SaldoPendiente = 0;
                    factura.Estado = "Pagada";
                }
                else
                {
                    factura.SaldoPendiente -= montoDisponible;
                    factura.Estado = "Abonada";
                    montoDisponible = 0;
                }
            }

            if (montoDisponible > 0)
            {
                cliente.SaldoAFavor += montoDisponible;
            }

            GuardarXml(todasFacturas, FacturasPath);
            GuardarXml(clientes, ClientesPath);
        }

        private T LeerXml<T>(string path) where T : class
        {
            if (!System.IO.File.Exists(path)) return null;

            XmlSerializer ser = new XmlSerializer(typeof(T));
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return (T)ser.Deserialize(fs);
            }
        }

        private void GuardarXml<T>(T data, string path)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            
            string folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                ser.Serialize(fs, data);
            }
        }

        [HttpGet("estado-cuenta/{nit?}")]
        public IActionResult GetEstadoCuenta(string nit = null)
        {
            var clientes = LeerXml<List<Cliente>>(ClientesPath) ?? new List<Cliente>();
            var facturas = LeerXml<List<FacturaXml>>(FacturasPath) ?? new List<FacturaXml>();

            var listaBusqueda = string.IsNullOrEmpty(nit) ? clientes.OrderBy(c => c.Nit).ToList() : clientes.Where(c => c.Nit == nit).ToList();

            var respuesta = new List<EstadoCuentaResponse>();

            foreach (var cliente in listaBusqueda)
            {
                var estado = new EstadoCuentaResponse
                {
                    Nit = cliente.Nit,
                    NombreCliente = cliente.Nombre,
                    SaldoActual = 0, 
                    Historial = new List<DetalleTransaccion>()
                };

                var fCliente = facturas.Where(f => f.Nit == cliente.Nit).ToList();
                
                foreach (var f in fCliente)
                {
                    estado.Historial.Add(new DetalleTransaccion {
                        Fecha = f.Fecha,
                        Tipo = "Cargo",
                        Descripcion = $"Fact. #{f.Numero}",
                        Monto = f.Monto
                    });
                }
                
                estado.SaldoActual = fCliente.Sum(f => f.SaldoPendiente) - cliente.SaldoAFavor;
                
                estado.Historial = estado.Historial
                    .OrderByDescending(h => DateTime.ParseExact(h.Fecha, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture))
                    .ToList();

                respuesta.Add(estado);
            }

            return Ok(respuesta);
        }
    }
}