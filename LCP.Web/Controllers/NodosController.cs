using LCP.Web.Data;
using LCP.Web.Models.Entities;
using LCP.Web.Models.ViewModels.Nodos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class NodosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NodosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var nodos = await _context.NodosDistribucion
                .Include(n => n.Provincia)
                .OrderBy(n => n.Provincia!.Nombre)
                .ThenBy(n => n.Nombre)
                .ToListAsync();

            return View(nodos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var nodo = await _context.NodosDistribucion
                .Include(n => n.Provincia)
                .Include(n => n.Pedidos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (nodo == null) return NotFound();

            return View(nodo);
        }

        public async Task<IActionResult> Create()
        {
            var provincias = await _context.Provincias.Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync();
            var model = new NodoViewModel
            {
                ProvinciasList = provincias.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Nombre })
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NodoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var nodo = new NodoDistribucion
                {
                    Nombre = model.Nombre,
                    Direccion = model.Direccion,
                    Ciudad = model.Ciudad,
                    CodigoPostal = model.CodigoPostal,
                    Telefono = model.Telefono,
                    HorarioAtencion = "Lunes a Viernes 08:00 a 18:00 hs",
                    TiempoEstimadoHoras = model.TiempoEstimadoHoras,
                    TarifaBase = model.TarifaBase,
                    Activo = model.Activo,
                    ProvinciaId = model.ProvinciaId
                };

                _context.Add(nodo);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = $"Nodo logístico '{nodo.Nombre}' creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            var provincias = await _context.Provincias.Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync();
            model.ProvinciasList = provincias.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Nombre });
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nodo = await _context.NodosDistribucion.FindAsync(id);
            if (nodo == null) return NotFound();

            var provincias = await _context.Provincias.Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync();
            var model = new NodoViewModel
            {
                Id = nodo.Id,
                Nombre = nodo.Nombre,
                Direccion = nodo.Direccion,
                Ciudad = nodo.Ciudad,
                CodigoPostal = nodo.CodigoPostal,
                Telefono = nodo.Telefono,
                TiempoEstimadoHoras = nodo.TiempoEstimadoHoras,
                TarifaBase = nodo.TarifaBase,
                Activo = nodo.Activo,
                ProvinciaId = nodo.ProvinciaId,
                ProvinciasList = provincias.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Nombre, Selected = p.Id == nodo.ProvinciaId })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NodoViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var nodo = await _context.NodosDistribucion.FindAsync(id);
                    if (nodo == null) return NotFound();

                    nodo.Nombre = model.Nombre;
                    nodo.Direccion = model.Direccion;
                    nodo.Ciudad = model.Ciudad;
                    nodo.CodigoPostal = model.CodigoPostal;
                    nodo.Telefono = model.Telefono;
                    nodo.TiempoEstimadoHoras = model.TiempoEstimadoHoras;
                    nodo.TarifaBase = model.TarifaBase;
                    nodo.Activo = model.Activo;
                    nodo.ProvinciaId = model.ProvinciaId;

                    _context.Update(nodo);
                    await _context.SaveChangesAsync();

                    TempData["MensajeExito"] = $"Nodo '{nodo.Nombre}' actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.NodosDistribucion.Any(e => e.Id == model.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            var provincias = await _context.Provincias.Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync();
            model.ProvinciasList = provincias.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Nombre });
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var nodo = await _context.NodosDistribucion.Include(n => n.Pedidos).FirstOrDefaultAsync(n => n.Id == id);
            if (nodo == null)
                return Json(new { success = false, message = "Nodo no encontrado." });

            if (nodo.Pedidos.Any(p => p.Estado != Models.Enums.EstadoPedido.Entregado && p.Estado != Models.Enums.EstadoPedido.Cancelado))
            {
                return Json(new { success = false, message = $"No se puede desactivar el nodo '{nodo.Nombre}' porque posee pedidos en tránsito activos." });
            }

            nodo.Activo = false;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"El nodo '{nodo.Nombre}' fue desactivado correctamente." });
        }

        // Endpoint para Dropdown Anidado (Permitido para Checkout y vistas públicas)
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetNodosPorProvincia(int provinciaId)
        {
            var nodos = await _context.NodosDistribucion
                .Where(n => n.ProvinciaId == provinciaId && n.Activo)
                .OrderBy(n => n.Nombre)
                .Select(n => new
                {
                    id = n.Id,
                    nombre = $"{n.Nombre} (${n.TarifaBase:N0})",
                    tarifa = n.TarifaBase,
                    tiempoHoras = n.TiempoEstimadoHoras,
                    ciudad = n.Ciudad,
                    direccion = n.Direccion
                })
                .ToListAsync();

            return Json(nodos);
        }
    }
}
