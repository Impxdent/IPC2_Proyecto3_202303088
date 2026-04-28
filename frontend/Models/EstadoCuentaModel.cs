using System.Collections.Generic;

namespace IPC2_Proyecto3_202303088.frontend.Models
{
    public class EstadoCuentaResponse
    {
        public string Nit { get; set; }
        public string NombreCliente { get; set; }
        public decimal SaldoActual { get; set; }
        public List<DetalleTransaccion> Historial { get; set; } = new List<DetalleTransaccion>();
    }

    public class DetalleTransaccion
    {
        public string Fecha { get; set; }
        public string Tipo { get; set; } 
        public string Descripcion { get; set; } 
        public decimal Monto { get; set; }
    }
}