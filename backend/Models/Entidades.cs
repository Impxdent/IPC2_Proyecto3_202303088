using System;
using System.Collections.Generic;

namespace IPC2_Proyecto3_202303088.backend.Models
{
    public class ClienteDominio
    {
        public string Nit { get; set; }
        public string Nombre { get; set; }
        public decimal SaldoAFavor { get; set; } = 0;
        
        public List<FacturaDominio> Facturas { get; set; } = new List<FacturaDominio>();
        public List<PagoDominio> Pagos { get; set; } = new List<PagoDominio>();
    }

    public class BancoDominio
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
    }

    public class FacturaDominio
    {
        public string Numero { get; set; }
        public string NitCliente { get; set; }
        public DateTime Fecha { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string Estado { get; set; } 

        public bool EstaPagada => SaldoPendiente <= 0;
    }

    public class PagoDominio
    {
        public string CodigoBanco { get; set; }
        public string NitCliente { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
    }
}