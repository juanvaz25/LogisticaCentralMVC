using System.Security.Claims;
using LCP.Web.Models.ViewModels.Reportes;
using LCP.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LCP.Web.Controllers
{
    [Authorize(Roles = "Vendedor,Administrador")]
    public class ReportesController : Controller
    {
        private readonly IEstadisticasService _estadisticasService;

        public ReportesController(IEstadisticasService estadisticasService)
        {
            _estadisticasService = estadisticasService;
        }

        public async Task<IActionResult> Estadisticas(int? estadoId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole("Administrador");
            var vendedorFiltro = esAdmin ? null : userId;

            // 1. Ejecutar Stored Procedure 1: Estadísticas y KPIs de Ventas
            var estadisticasSp = await _estadisticasService.ObtenerEstadisticasSpAsync(vendedorFiltro);

            // 2. Ejecutar Stored Procedure 2: Listado de Pedidos filtrados por Estado
            var listadoPedidosSp = await _estadisticasService.ListarPedidosReporteSpAsync(estadoId, vendedorFiltro);

            var model = new ReporteVentasViewModel
            {
                Estadisticas = estadisticasSp,
                PedidosRecientesSp = listadoPedidosSp,
                EstadoFiltroId = estadoId,
                TituloReporte = esAdmin ? "Métricas Generales de la Red LCP (Stored Procedures)" : "Métricas de Ventas y Logística de mi Comercio"
            };

            return View(model);
        }
    }
}
