using System.Xml.Serialization;
using System.Collections.Generic;

namespace IPC2_Proyecto3_202303088.backend.Models
{
    [XmlRoot("TRANSACCIONES")]
    public class TransaccionesData
    {
        [XmlArray("FACTURAS")]
        [XmlArrayItem("FACTURA")]
        public List<FacturaXml> Facturas { get; set; }

        [XmlArray("PAGOS")]
        [XmlArrayItem("PAGO")]
        public List<PagoXml> Pagos { get; set; }
    }

    public class FacturaXml
    {
        [XmlElement("NUMERO")]
        public string Numero { get; set; }

        [XmlElement("NIT")]
        public string Nit { get; set; }

        [XmlElement("FECHA")]
        public string Fecha { get; set; }

        [XmlElement("MONTO")]
        public decimal Monto { get; set; }

        [XmlIgnore]
        public decimal SaldoPendiente { get; set; }

        [XmlIgnore]
        public string Estado { get; set; } = "Pendiente";
    }

    public class PagoXml
    {
        [XmlElement("CODIGO")]
        public string CodigoBanco { get; set; }

        [XmlElement("FECHA")]
        public string Fecha { get; set; }

        [XmlElement("NIT")]
        public string Nit { get; set; }

        [XmlElement("MONTO")]
        public decimal Monto { get; set; }
    }
}