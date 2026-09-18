using LCP.Web.Data;
using LCP.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categorias = await _context.Categorias
                .Include(c => c.Productos)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(categorias);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        public IActionResult Create()
        {
            return View(new Categoria());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Icono,Activo")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                categoria.FechaCreacion = DateTime.UtcNow;
                _context.Add(categoria);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = $"Categoría '{categoria.Nombre}' creada con éxito.";
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Icono,Activo,FechaCreacion")] Categoria categoria)
        {
            if (id != categoria.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                    TempData["MensajeExito"] = $"Categoría '{categoria.Nombre}' actualizada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categorias.Any(e => e.Id == categoria.Id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _context.Categorias.Include(c => c.Productos).FirstOrDefaultAsync(c => c.Id == id);
            if (categoria == null)
                return Json(new { success = false, message = "Categoría no encontrada." });

            if (categoria.Productos.Any(p => p.Activo))
            {
                return Json(new { success = false, message = $"No se puede eliminar la categoría '{categoria.Nombre}' porque contiene productos activos asociados." });
            }

            categoria.Activo = false;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"La categoría '{categoria.Nombre}' fue desactivada correctamente." });
        }
    }
}
