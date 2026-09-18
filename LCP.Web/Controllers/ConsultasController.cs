using System.Security.Claims;
using LCP.Web.Data;
using LCP.Web.Models.Entities;
using LCP.Web.Models.ViewModels.Consultas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Controllers
{
    [Authorize]
    public class ConsultasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConsultasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole("Administrador");

            var recibidas = await _context.ConsultasSoporte
                .Include(c => c.Emisor)
                .Include(c => c.Producto)
                .Where(c => esAdmin || c.ReceptorId == userId || (c.ReceptorId == null && esAdmin))
                .OrderByDescending(c => c.FechaEnvio)
                .ToListAsync();

            var enviadas = await _context.ConsultasSoporte
                .Include(c => c.Receptor)
                .Include(c => c.Producto)
                .Where(c => c.EmisorId == userId)
                .OrderByDescending(c => c.FechaEnvio)
                .ToListAsync();

            ViewBag.Recibidas = recibidas;
            ViewBag.Enviadas = enviadas;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Nueva(int? productoId = null, string? receptorId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var model = new ConsultaCreateViewModel
            {
                ProductoId = productoId,
                ReceptorId = receptorId
            };

            if (productoId.HasValue)
            {
                var producto = await _context.Productos.Include(p => p.Vendedor).FirstOrDefaultAsync(p => p.Id == productoId.Value);
                if (producto != null)
                {
                    model.TituloProducto = producto.Nombre;
                    model.ReceptorId = producto.VendedorId;
                    model.NombreDestinatario = producto.Vendedor?.NombreComercio ?? producto.Vendedor?.NombreCompleto;
                    model.Asunto = $"Consulta sobre producto: {producto.Nombre}";
                }
            }

            await CargarDestinatariosAsync(model, userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Nueva(ConsultaCreateViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (ModelState.IsValid)
            {
                var consulta = new ConsultaSoporte
                {
                    Asunto = model.Asunto,
                    Mensaje = model.Mensaje,
                    FechaEnvio = DateTime.UtcNow,
                    Respondido = false,
                    EsUrgente = model.EsUrgente,
                    EmisorId = userId,
                    ReceptorId = string.IsNullOrWhiteSpace(model.ReceptorId) ? null : model.ReceptorId,
                    ProductoId = model.ProductoId
                };

                _context.Add(consulta);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = "Consulta enviada correctamente. El destinatario recibirá una notificación.";
                return RedirectToAction(nameof(Index));
            }

            await CargarDestinatariosAsync(model, userId);
            return View(model);
        }

        private async Task CargarDestinatariosAsync(ConsultaCreateViewModel model, string? currentUserId)
        {
            var lista = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
            {
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = "",
                    Text = "🏛️ Soporte Central / Administrador LCP",
                    Selected = string.IsNullOrEmpty(model.ReceptorId)
                }
            };

            // Vendedores a los que el usuario ya les compró
            var misVendedoresComprados = new List<string>();
            if (!string.IsNullOrEmpty(currentUserId))
            {
                misVendedoresComprados = await _context.Pedidos
                    .Where(p => p.CompradorId == currentUserId)
                    .Select(p => p.VendedorId)
                    .Distinct()
                    .ToListAsync();
            }

            var vendedores = await _context.Users
                .Where(u => u.EsVendedorActivo && u.Id != currentUserId)
                .OrderBy(u => u.NombreComercio ?? u.Nombre)
                .ToListAsync();

            foreach (var v in vendedores)
            {
                var esComprado = misVendedoresComprados.Contains(v.Id);
                var nombre = v.NombreComercio ?? v.NombreCompleto;
                var etiqueta = esComprado ? $"🏪 {nombre} ⭐ (Le compraste recientemente)" : $"🏪 {nombre}";

                lista.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = v.Id,
                    Text = etiqueta,
                    Selected = model.ReceptorId == v.Id
                });
            }

            model.DestinatariosList = lista;
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole("Administrador");

            var consulta = await _context.ConsultasSoporte
                .Include(c => c.Emisor)
                .Include(c => c.Receptor)
                .Include(c => c.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consulta == null)
                return NotFound();

            if (!esAdmin && consulta.EmisorId != userId && consulta.ReceptorId != userId && consulta.ReceptorId != null)
                return Forbid();

            var model = new ConsultaResponderViewModel
            {
                ConsultaId = consulta.Id,
                Consulta = consulta
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Responder(ConsultaResponderViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole("Administrador");

            var consulta = await _context.ConsultasSoporte
                .Include(c => c.Emisor)
                .Include(c => c.Receptor)
                .Include(c => c.Producto)
                .FirstOrDefaultAsync(c => c.Id == model.ConsultaId);

            if (consulta == null)
                return NotFound();

            if (!esAdmin && consulta.ReceptorId != userId && consulta.ReceptorId != null)
                return Forbid();

            if (string.IsNullOrWhiteSpace(model.Respuesta))
            {
                ModelState.AddModelError("Respuesta", "La respuesta no puede estar vacía.");
                model.Consulta = consulta;
                return View("Detalle", model);
            }

            consulta.Respuesta = model.Respuesta;
            consulta.Respondido = true;
            consulta.FechaRespuesta = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Respuesta enviada satisfactoriamente.";
            return RedirectToAction(nameof(Detalle), new { id = consulta.Id });
        }
    }
}
