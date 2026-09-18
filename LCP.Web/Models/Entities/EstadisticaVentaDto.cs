namespace LCP.Web.Models.Entities
{
    public class EstadisticaVentaDto
    {
        public int TotalPedidos { get; set; }
        public decimal TotalVentas { get; set; }
        public int PedidosEnProceso { get; set; }
        public int PedidosEnPreparacion { get; set; }
        public int PedidosEnCamino { get; set; }
        public int PedidosEntregados { get; set; }
        public int PedidosCancelados { get; set; }
        public int TotalProductos { get; set; }
        public string? CategoriaMasVendida { get; set; }
    }

    public class PedidoReporteSpDto
    {
        public int PedidoId { get; set; }
        public string CodigoSeguimiento { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; }
        public string NombreComprador { get; set; } = string.Empty;
        public string NombreVendedor { get; set; } = string.Empty;
        public string CiudadEntrega { get; set; } = string.Empty;
        public string ProvinciaEntrega { get; set; } = string.Empty;
        public int EstadoId { get; set; }
        public string EstadoNombre { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int CantidadItems { get; set; }
    }
}
