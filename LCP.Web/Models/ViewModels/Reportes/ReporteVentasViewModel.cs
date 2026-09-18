using LCP.Web.Models.Entities;

namespace LCP.Web.Models.ViewModels.Reportes
{
    public class ReporteVentasViewModel
    {
        public EstadisticaVentaDto Estadisticas { get; set; } = new();
        public List<PedidoReporteSpDto> PedidosRecientesSp { get; set; } = new();
        public string? VendedorFiltroId { get; set; }
        public int? EstadoFiltroId { get; set; }
        public string TituloReporte { get; set; } = "Estadísticas y Métricas de Operaciones LCP";
    }
}
