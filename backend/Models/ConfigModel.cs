using System.Xml.Serialization;
using System.Collections.Generic;

namespace IPC2_Proyecto3_202303088.backend.Models
{
    [XmlRoot("CONFIG")]
    public class ConfigData
    {
        [XmlArray("CLIENTES")]
        [XmlArrayItem("CLIENTE")]
        public List<Cliente> Clientes { get; set; }

        [XmlArray("BANCOS")]
        [XmlArrayItem("BANCO")]
        public List<Banco> Bancos { get; set; }
    }

    public class Cliente
    {
        [XmlElement("NIT")]
        public string Nit { get; set; }

        [XmlElement("NOMBRE")]
        public string Nombre { get; set; }

        [XmlIgnore]
        public decimal SaldoAFavor { get; set; } = 0;
    }

    public class Banco
    {
        [XmlElement("CODIGO")]
        public string Codigo { get; set; }

        [XmlElement("NOMBRE")]
        public string Nombre { get; set; }
    }
}