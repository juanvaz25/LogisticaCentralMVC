using System.Security.Claims;
using System.Text.Json;
using LCP.Web.Data;
using LCP.Web.Models.Entities;
using LCP.Web.Models.Enums;
using LCP.Web.Models.ViewModels.Pedidos;
using LCP.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPedidoService _pedidoService;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string SessionCartKey = "LCP_CART_SESSION";

        public PedidosController(
            ApplicationDbContext context,
            IPedidoService pedidoService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _pedidoService = pedidoService;
            _userManager = userManager;
        }

        private List<CarritoItemViewModel> ObtenerCarrito()
        {
            var cartJson = HttpContext.Session.GetString(SessionCartKey);
            if (string.IsNullOrEmpty(cartJson))
                return new List<CarritoItemViewModel>();

            return JsonSerializer.Deserialize<List<CarritoItemViewModel>>(cartJson) ?? new List<CarritoItemViewModel>();
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (User.IsInRole("Vendedor"))
            {
                TempData["MensajeInfo"] = "Tu perfil actual es de Vendedor. La función de compras está reservada para cuentas de Comprador.";
                return RedirectToAction("Index", "Vendedor");
            }
            if (User.IsInRole("Administrador"))
            {
                TempData["MensajeInfo"] = "Tu perfil actual es de Administrador. La función de compras está reservada para cuentas de Comprador.";
                return RedirectToAction("Index", "Admin");
            }

            var items = ObtenerCarrito();
            if (!items.Any())
            {
                TempData["MensajeError"] = "Tu carrito está vacío. Agrega productos antes de finalizar la compra.";
                return RedirectToAction("Index", "Catalogo");
            }

            var user = await _userManager.GetUserAsync(User);
            var provincias = await _context.Provincias.Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync();
            var primeraProvinciaId = provincias.FirstOrDefault()?.Id ?? 0;
            var nodos = await _context.NodosDistribucion.Where(n => n.ProvinciaId == primeraProvinciaId && n.Activo).ToListAsync();

            var model = new CheckoutViewModel
            {
                Items = items,
                Subtotal = items.Sum(i => i.Subtotal),
                CostoEnvio = nodos.FirstOrDefault()?.TarifaBase ?? 2500m,
                Total = items.Sum(i => i.Subtotal) + (nodos.FirstOrDefault()?.TarifaBase ?? 2500m),
                DireccionEntrega = user?.Direccion ?? string.Empty,
                CiudadEntrega = user?.Ciudad ?? string.Empty,
                TelefonoContacto = user?.PhoneNumber ?? string.Empty,
                ProvinciaId = primeraProvinciaId,
                ProvinciasList = provincias.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Nombre }),
                NodosList = nodos.Select(n => new SelectListItem { Value = n.Id.ToString(), Text = $"{n.Nombre} (${n.TarifaBase:N0})" })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (User.IsInRole("Vendedor"))
            {
                TempData["MensajeInfo"] = "Tu perfil actual es de Vendedor. La función de compras está reservada para cuentas de Comprador.";
                return RedirectToAction("Index", "Vendedor");
            }
            if (User.IsInRole("Administrador"))
            {
                TempData["MensajeInfo"] = "Tu perfil actual es de Administrador. La función de compras está reservada para cuentas de Comprador.";
                return RedirectToAction("Index", "Admin");
            }

            var items = ObtenerCarrito();
            if (!items.Any())
            {
                TempData["MensajeError"] = "Tu carrito está vacío.";
                return RedirectToAction("Index", "Catalogo");
            }

            model.Items = items;

            if (!ModelState.IsValid)
            {
                var provincias = await _context.Provincias.Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync();
                var nodos = await _context.NodosDistribucion.Where(n => n.ProvinciaId == model.ProvinciaId && n.Activo).ToListAsync();

                model.ProvinciasList = provincias.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Nombre, Selected = p.Id == model.ProvinciaId });
                model.NodosList = nodos.Select(n => new SelectListItem { Value = n.Id.ToString(), Text = $"{n.Nombre} (${n.TarifaBase:N0})", Selected = n.Id == model.NodoDistribucionId });
                model.Subtotal = items.Sum(i => i.Subtotal);
                model.CostoEnvio = nodos.FirstOrDefault(n => n.Id == model.NodoDistribucionId)?.TarifaBase ?? 2500m;
                model.Total = model.Subtotal + model.CostoEnvio;

                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var pedido = await _pedidoService.CrearPedidoDesdeCheckoutAsync(model, userId);

                // Vaciar Carrito
                HttpContext.Session.Remove(SessionCartKey);

                TempData["MensajeExito"] = $"¡Pedido {pedido.CodigoSeguimiento} creado con éxito! Puedes seguir el estado en tiempo real.";
                return RedirectToAction(nameof(Detalle), new { id = pedido.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al procesar el pedido: {ex.Message}");

                var provincias = await _context.Provincias.Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync();
                var nodos = await _context.NodosDistribucion.Where(n => n.ProvinciaId == model.ProvinciaId && n.Activo).ToListAsync();
                model.ProvinciasList = provincias.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Nombre });
                model.NodosList = nodos.Select(n => new SelectListItem { Value = n.Id.ToString(), Text = $"{n.Nombre} (${n.TarifaBase:N0})" });

                return View(model);
            }
        }

        public async Task<IActionResult> MisPedidos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var pedidos = await _context.Pedidos
                .Include(p => p.Vendedor)
                .Include(p => p.NodoDistribucion)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.CompradorId == userId)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            return View(pedidos);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole(TipoUsuario.Administrador.ToString());
            var esVendedor = User.IsInRole(TipoUsuario.Vendedor.ToString());

            var pedido = await _context.Pedidos
                .Include(p => p.Comprador)
                .Include(p => p.Vendedor)
                .Include(p => p.NodoDistribucion)
                    .ThenInclude(n => n!.Provincia)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(p => p.Historial.OrderByDescending(h => h.Fecha))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound();

            // Verificar permisos de acceso
            if (!esAdmin && pedido.CompradorId != userId && pedido.VendedorId != userId)
                return Forbid();

            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, string motivoCancelacion)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(motivoCancelacion))
            {
                TempData["MensajeError"] = "Debe indicar obligatoriamente el motivo de cancelación.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
                return NotFound();

            var esAdmin = User.IsInRole(TipoUsuario.Administrador.ToString());
            if (!esAdmin && pedido.CompradorId != userId && pedido.VendedorId != userId)
                return Forbid();

            if (pedido.Estado == EstadoPedido.Entregado)
            {
                TempData["MensajeError"] = "No se puede cancelar un pedido que ya ha sido entregado.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            await _pedidoService.CambiarEstadoAsync(id, EstadoPedido.Cancelado, motivoCancelacion, "Cancelado por el usuario", userId);

            // Reintegrar stock de los productos
            var detalles = await _context.DetallesPedido.Where(d => d.PedidoId == id).ToListAsync();
            foreach (var d in detalles)
            {
                var prod = await _context.Productos.FindAsync(d.ProductoId);
                if (prod != null)
                {
                    prod.Stock += d.Cantidad;
                }
            }
            await _context.SaveChangesAsync();

            TempData["MensajeInfo"] = "El pedido ha sido cancelado y se notificó el motivo registrado.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        [AllowAnonymous]
        public async Task<IActionResult> ValidarEntregaQr(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.Error = "Token de confirmación QR no especificado o inválido.";
                return View();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Comprador)
                .Include(p => p.Vendedor)
                .Include(p => p.NodoDistribucion)
                .FirstOrDefaultAsync(p => p.TokenEntregaQr == token.Trim());

            if (pedido == null)
            {
                ViewBag.Error = "No se encontró ningún pedido con el código de seguridad QR escaneado.";
                return View();
            }

            if (pedido.Estado == EstadoPedido.Entregado)
            {
                ViewBag.Mensaje = "Este pedido ya fue validado y entregado con anterioridad.";
                ViewBag.Pedido = pedido;
                return View();
            }

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Escáner QR Repartidor";
            await _pedidoService.ConfirmarEntregaPorQrAsync(token, usuarioId);

            ViewBag.Exito = true;
            ViewBag.Mensaje = $"¡Entrega confirmada con éxito para el pedido {pedido.CodigoSeguimiento}!";
            ViewBag.Pedido = pedido;

            return View();
        }

        public async Task<IActionResult> Comprobante(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Comprador)
                .Include(p => p.Vendedor)
                .Include(p => p.NodoDistribucion)
                    .ThenInclude(n => n!.Provincia)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound();

            return View(pedido);
        }
    }
}
