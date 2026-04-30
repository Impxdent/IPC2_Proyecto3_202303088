using System.Xml.Serialization;
using System.Collections.Generic;

namespace IPC2_Proyecto3_202303088.backend.Models
{
    [XmlRoot("config")]
    public class ConfigData
    {
        [XmlArray("clientes")]
        [XmlArrayItem("cliente")]
        public List<Cliente> Clientes { get; set; }

        [XmlArray("bancos")]
        [XmlArrayItem("banco")]
        public List<Banco> Bancos { get; set; }
    }

    public class Cliente
    {
        [XmlElement("nit")]
        public string Nit { get; set; }

        [XmlElement("nombre")]
        public string Nombre { get; set; }

        [XmlIgnore]
        public decimal SaldoAFavor { get; set; } = 0;
    }

    public class Banco
    {
        [XmlElement("codigo")]
        public string Codigo { get; set; }

        [XmlElement("nombre")]
        public string Nombre { get; set; }
    }
}