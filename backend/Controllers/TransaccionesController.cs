using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;
using IPC2_Proyecto3_202303088.backend.Models;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace IPC2_Proyecto3_202303088.backend.Controllers
{
    [ApiController]
    [Route("/")] 
    public class TransaccionesController : ControllerBase
    {
        private readonly string FacturasPath = Path.Combine("DataStorage", "MasterFacturas.xml");
        private readonly string ClientesPath = Path.Combine("DataStorage", "MasterClientes.xml");
        private readonly string BancosPath = Path.Combine("DataStorage", "MasterBancos.xml");
        private readonly string PagosPath = Path.Combine("DataStorage", "MasterPagos.xml");

        [HttpPost("grabarTransaccion")]
        public IActionResult CargarTransacciones([FromBody] string xmlContent)
        {
            try {
                XmlSerializer serializer = new XmlSerializer(typeof(TransaccionesData));
                TransaccionesData data;
                using (StringReader reader = new StringReader(xmlContent)) {
                    data = (TransaccionesData)serializer.Deserialize(reader);
                }

                var facturasMaster = LeerXml<List<FacturaXml>>(FacturasPath) ?? new List<FacturaXml>();
                foreach (var f in data.Facturas) {
                    if (!facturasMaster.Any(x => x.NumeroFactura == f.NumeroFactura)) {
                        f.SaldoPendiente = f.Valor;
                        f.Estado = "Pendiente";
                        facturasMaster.Add(f);
                    }
                }
                GuardarXml(facturasMaster, FacturasPath);

                foreach (var pago in data.Pagos) {
                    AplicarPagoFIFO(pago); 
                    
                    var pagosMaster = LeerXml<List<PagoXml>>(PagosPath) ?? new List<PagoXml>();
                    pagosMaster.Add(pago);
                    GuardarXml(pagosMaster, PagosPath);
                }

                var respuesta = new RespuestaTransaccionXml {
                    Mensaje = "Archivo procesado exitosamente",
                    TotalFacturas = data.Facturas.Count,
                    TotalPagos = data.Pagos.Count
                };

                return Ok(respuesta); 
            }
            catch (Exception ex) {
                return BadRequest(new { mensaje = "Error: " + ex.Message });
            }
        }

        private string SerializarAXml<T>(T objeto)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            using (StringWriter writer = new StringWriter())
            {
                ser.Serialize(writer, objeto);
                return writer.ToString();
            }
        }

        private void ProcesarFacturas(List<FacturaXml> nuevas)
        {
            var actuales = LeerXml<List<FacturaXml>>(FacturasPath) ?? new List<FacturaXml>();
            foreach (var f in nuevas)
            {
                // Verificamos si la factura ya existe por número
                if (!actuales.Any(x => x.NumeroFactura == f.NumeroFactura))
                {
                    f.SaldoPendiente = f.Valor; // El saldo inicial es el valor total
                    f.Estado = "Pendiente";
                    actuales.Add(f);
                }
            }
            GuardarXml(actuales, FacturasPath);
        }

        private void ProcesarPagosHistorial(List<PagoXml> nuevosPagos)
        {
            var historial = LeerXml<List<PagoXml>>(PagosPath) ?? new List<PagoXml>();
            historial.AddRange(nuevosPagos);
            GuardarXml(historial, PagosPath);
        }

        private void AplicarPagoFIFO(PagoXml pago)
        {
            var todasFacturas = LeerXml<List<FacturaXml>>(FacturasPath) ?? new List<FacturaXml>();
            var clientes = LeerXml<List<Cliente>>(ClientesPath) ?? new List<Cliente>();
            
            // Buscamos al cliente (Usando el nombre de propiedad 'Nit' definida en tu PagoXml)
            var cliente = clientes.FirstOrDefault(c => c.Nit == pago.Nit);
            if (cliente == null) return; 

            // Obtener facturas pendientes del cliente ordenadas por fecha (FIFO)
            var pendientes = todasFacturas
                .Where(f => f.NitCliente == pago.Nit && f.Estado != "Pagada")
                .OrderBy(f => {
                    DateTime.TryParseExact(f.Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt);
                    return dt;
                })
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

            // Si sobró dinero del pago, se va a saldo a favor
            if (montoDisponible > 0)
            {
                cliente.SaldoAFavor += montoDisponible;
            }

            GuardarXml(todasFacturas, FacturasPath);
            GuardarXml(clientes, ClientesPath);
        }

        [HttpGet("devolverEstadoCuenta/{nit?}")]
        public IActionResult GetEstadoCuenta(string nit = null)
        {
            var clientes = LeerXml<List<Cliente>>(ClientesPath) ?? new List<Cliente>();
            var facturas = LeerXml<List<FacturaXml>>(FacturasPath) ?? new List<FacturaXml>();

            var listaBusqueda = string.IsNullOrEmpty(nit) 
                ? clientes.OrderBy(c => c.Nit).ToList() 
                : clientes.Where(c => c.Nit == nit).ToList();

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

                var fCliente = facturas.Where(f => f.NitCliente == cliente.Nit).ToList();
                
                foreach (var f in fCliente)
                {
                    estado.Historial.Add(new DetalleTransaccion {
                        Fecha = f.Fecha,
                        Tipo = "Cargo",
                        Descripcion = $"Fact. #{f.NumeroFactura}",
                        Monto = f.Valor
                    });
                }
                
                // Saldo actual = Suma de pendientes - saldo a favor
                estado.SaldoActual = fCliente.Sum(f => f.SaldoPendiente) - cliente.SaldoAFavor;
                
                estado.Historial = estado.Historial
                    .OrderByDescending(h => {
                        DateTime.TryParseExact(h.Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt);
                        return dt;
                    })
                    .ToList();

                respuesta.Add(estado);
            }

            return Ok(respuesta);
        }

        [HttpGet("devolverResumenPagos")]
        public IActionResult DevolverResumenPagos([FromQuery] int mes = 4, [FromQuery] int anio = 2026)
        {
            try
            {
                var pagos = LeerXml<List<PagoXml>>(PagosPath) ?? new List<PagoXml>();
                var bancos = LeerXml<List<Banco>>(BancosPath) ?? new List<Banco>();

                var reporte = bancos.Select(b => new {
                    Nombre = b.Nombre,
                    Total = pagos.Where(p => {
                        if (DateTime.TryParseExact(p.Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                        {
                            return p.Codigo == b.Codigo && fecha.Month == mes && fecha.Year == anio;
                        }
                        return false;
                    }).Sum(p => p.Monto)
                }).ToList();

                return Ok(reporte);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        private T LeerXml<T>(string path) where T : class
        {
            if (!System.IO.File.Exists(path)) return null;
            try {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    return (T)ser.Deserialize(fs);
                }
            } catch { return null; }
        }

        private void GuardarXml<T>(T data, string path)
        {
            try {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                string folder = Path.GetDirectoryName(path);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    ser.Serialize(fs, data);
                }
            } catch { }
        }
    }
}