using LCP.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LCP.Web.Controllers.Api
{
    [Route("api/v1/tracking")]
    [ApiController]
    public class ApiTrackingController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;

        public ApiTrackingController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        // GET: api/v1/tracking/{codigo}
        [HttpGet("{codigo}")]
        public async Task<IActionResult> GetTracking(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return BadRequest(new { error = "El código de seguimiento es obligatorio." });

            var pedido = await _pedidoService.ObtenerPorCodigoTrackingAsync(codigo);
            if (pedido == null)
                return NotFound(new { error = $"No se encontró ningún envío con el código '{codigo}'." });

            return Ok(new
            {
                pedido.CodigoSeguimiento,
                FechaPedido = pedido.FechaPedido.ToString("yyyy-MM-dd HH:mm"),
                FechaEntregaEstimada = pedido.FechaEntregaEstimada?.ToString("yyyy-MM-dd HH:mm"),
                FechaEntregaReal = pedido.FechaEntregaReal?.ToString("yyyy-MM-dd HH:mm"),
                EstadoId = (int)pedido.Estado,
                Estado = pedido.Estado.ToString(),
                pedido.MotivoCancelacion,
                Destino = $"{pedido.DireccionEntrega}, {pedido.CiudadEntrega}, {pedido.ProvinciaEntrega}",
                NodoLogistico = pedido.NodoDistribucion?.Nombre,
                Comprador = pedido.Comprador?.NombreCompleto,
                Vendedor = pedido.Vendedor?.NombreComercio ?? pedido.Vendedor?.NombreCompleto,
                Total = pedido.Total,
                HistorialEventos = pedido.Historial.Select(h => new
                {
                    Fecha = h.Fecha.ToString("yyyy-MM-dd HH:mm"),
                    h.Ubicacion,
                    h.Descripcion,
                    Estado = h.Estado.ToString(),
                    h.OperadorResponsable
                })
            });
        }
    }
}
