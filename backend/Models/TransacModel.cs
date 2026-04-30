using System.Xml.Serialization;
using System.Collections.Generic;

namespace IPC2_Proyecto3_202303088.backend.Models
{
    [XmlRoot("transacciones")]
    public class TransaccionesData
    {
        [XmlArray("facturas")]
        [XmlArrayItem("factura")]
        public List<FacturaXml> Facturas { get; set; }

        [XmlArray("pagos")]
        [XmlArrayItem("pago")]
        public List<PagoXml> Pagos { get; set; }
    }

    public class FacturaXml
    {
        [XmlElement("numeroFactura")]
        public string NumeroFactura { get; set; }

        [XmlElement("NITcliente")]
        public string NitCliente { get; set; }

        [XmlElement("fecha")]
        public string Fecha { get; set; }

        [XmlElement("valor")]
        public decimal Valor { get; set; }

        [XmlIgnore] 
        public decimal SaldoPendiente { get; set; } 

        [XmlIgnore] 
        public string Estado { get; set; } = "Pendiente";
    }

    public class PagoXml
    {
        [XmlElement("codigoBanco")]
        public string Codigo { get; set; }

        [XmlElement("fecha")]
        public string Fecha { get; set; }

        [XmlElement("NITcliente")]
        public string Nit { get; set; }

        [XmlElement("valor")]
        public decimal Monto { get; set; }
    }

    [XmlRoot("respuesta")]
    public class RespuestaTransaccionXml {
        [XmlElement("mensaje")]
        public string Mensaje { get; set; }
        
        [XmlElement("totalFacturas")]
        public int TotalFacturas { get; set; }
        
        [XmlElement("totalPagos")]
        public int TotalPagos { get; set; }
    }

    public class FacturaResumen
    {
        public string NumeroFactura { get; set; }
        public string Estado { get; set; }
        public decimal SaldoRestante { get; set; }
    }
}