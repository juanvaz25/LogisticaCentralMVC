using System.Security.Claims;
using LCP.Web.Data;
using LCP.Web.Models.Enums;
using LCP.Web.Models.ViewModels.Pedidos;
using LCP.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Controllers
{
    [Authorize(Roles = "Vendedor,Administrador")]
    public class VendedorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPedidoService _pedidoService;
        private readonly IEstadisticasService _estadisticasService;

        public VendedorController(
            ApplicationDbContext context,
            IPedidoService pedidoService,
            IEstadisticasService estadisticasService)
        {
            _context = context;
            _pedidoService = pedidoService;
            _estadisticasService = estadisticasService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole(TipoUsuario.Administrador.ToString());
            var vendedorFiltro = esAdmin ? null : userId;

            var estadisticas = await _estadisticasService.ObtenerEstadisticasSpAsync(vendedorFiltro);

            var pedidosRecientes = await _context.Pedidos
                .Include(p => p.Comprador)
                .Include(p => p.Detalles)
                .Where(p => esAdmin || p.VendedorId == userId)
                .OrderByDescending(p => p.FechaPedido)
                .Take(5)
                .ToListAsync();

            var productosMasVistos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => esAdmin || p.VendedorId == userId)
                .OrderByDescending(p => p.NumeroVisitas)
                .Take(5)
                .ToListAsync();

            ViewBag.Estadisticas = estadisticas;
            ViewBag.PedidosRecientes = pedidosRecientes;
            ViewBag.ProductosMasVistos = productosMasVistos;

            return View();
        }

        public async Task<IActionResult> Pedidos(int? estadoId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole(TipoUsuario.Administrador.ToString());

            var query = _context.Pedidos
                .Include(p => p.Comprador)
                .Include(p => p.NodoDistribucion)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => esAdmin || p.VendedorId == userId);

            if (estadoId.HasValue && estadoId.Value > 0)
            {
                query = query.Where(p => (int)p.Estado == estadoId.Value);
            }

            var pedidos = await query.OrderByDescending(p => p.FechaPedido).ToListAsync();
            ViewBag.EstadoSeleccionado = estadoId;

            return View(pedidos);
        }

        [HttpGet]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole(TipoUsuario.Administrador.ToString());

            var pedido = await _context.Pedidos
                .Include(p => p.NodoDistribucion)
                .FirstOrDefaultAsync(p => p.Id == id && (esAdmin || p.VendedorId == userId));

            if (pedido == null)
                return NotFound();

            var model = new CambiarEstadoPedidoViewModel
            {
                PedidoId = pedido.Id,
                CodigoSeguimiento = pedido.CodigoSeguimiento,
                NuevoEstado = pedido.Estado,
                Ubicacion = pedido.NodoDistribucion?.Nombre ?? "Centro Logístico LCP",
                MotivoObservacion = pedido.MotivoCancelacion
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(CambiarEstadoPedidoViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole(TipoUsuario.Administrador.ToString());

            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p => p.Id == model.PedidoId && (esAdmin || p.VendedorId == userId));

            if (pedido == null)
                return NotFound();

            if (model.NuevoEstado == EstadoPedido.Cancelado && string.IsNullOrWhiteSpace(model.MotivoObservacion))
            {
                ModelState.AddModelError("MotivoObservacion", "Debe ingresar una observación explicando el motivo de cancelación.");
                return View(model);
            }

            try
            {
                await _pedidoService.CambiarEstadoAsync(model.PedidoId, model.NuevoEstado, model.MotivoObservacion, model.Ubicacion, userId ?? "Vendedor");

                TempData["MensajeExito"] = $"Estado del pedido {pedido.CodigoSeguimiento} actualizado a '{model.NuevoEstado}'.";
                return RedirectToAction(nameof(Pedidos));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al actualizar estado: {ex.Message}");
                return View(model);
            }
        }

        public async Task<IActionResult> MisProductos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole(TipoUsuario.Administrador.ToString());

            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => esAdmin || p.VendedorId == userId)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return View(productos);
        }
    }
}
