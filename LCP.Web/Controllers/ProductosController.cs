using System.Security.Claims;
using LCP.Web.Data;
using LCP.Web.Models.Entities;
using LCP.Web.Models.Enums;
using LCP.Web.Models.ViewModels.Productos;
using LCP.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Controllers
{
    [Authorize(Roles = "Vendedor,Administrador")]
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storageService;

        public ProductosController(ApplicationDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole(TipoUsuario.Administrador.ToString());

            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Vendedor)
                .Where(p => esAdmin || p.VendedorId == userId)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return View(productos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Vendedor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null) return NotFound();

            return View(producto);
        }

        public async Task<IActionResult> Create()
        {
            var categorias = await _context.Categorias.Where(c => c.Activo).OrderBy(c => c.Nombre).ToListAsync();
            var model = new ProductoViewModel
            {
                CategoriasList = categorias.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre })
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (ModelState.IsValid)
            {
                string? imagenUrl = null;
                if (model.ImagenArchivo != null && model.ImagenArchivo.Length > 0)
                {
                    imagenUrl = await _storageService.GuardarArchivoAsync(model.ImagenArchivo, "productos");
                }

                var producto = new Producto
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    Precio = model.Precio,
                    Stock = model.Stock,
                    ImagenUrl = imagenUrl ?? "https://images.unsplash.com/photo-1587049352846-4a222e784d38?w=500&auto=format&fit=crop&q=60",
                    CodigoSku = model.CodigoSku,
                    PesoKg = model.PesoKg,
                    Activo = model.Activo,
                    Destacado = model.Destacado,
                    CategoriaId = model.CategoriaId,
                    VendedorId = userId,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Add(producto);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = $"¡Producto '{producto.Nombre}' publicado exitosamente!";
                return RedirectToAction(nameof(Index));
            }

            var categorias = await _context.Categorias.Where(c => c.Activo).OrderBy(c => c.Nombre).ToListAsync();
            model.CategoriasList = categorias.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre });
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole(TipoUsuario.Administrador.ToString());

            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id && (esAdmin || p.VendedorId == userId));
            if (producto == null) return NotFound();

            var categorias = await _context.Categorias.Where(c => c.Activo).OrderBy(c => c.Nombre).ToListAsync();
            var model = new ProductoViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                ImagenUrl = producto.ImagenUrl,
                CodigoSku = producto.CodigoSku,
                PesoKg = producto.PesoKg,
                Activo = producto.Activo,
                Destacado = producto.Destacado,
                CategoriaId = producto.CategoriaId,
                VendedorId = producto.VendedorId,
                CategoriasList = categorias.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre, Selected = c.Id == producto.CategoriaId })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductoViewModel model)
        {
            if (id != model.Id) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole(TipoUsuario.Administrador.ToString());

            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id && (esAdmin || p.VendedorId == userId));
            if (producto == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.ImagenArchivo != null && model.ImagenArchivo.Length > 0)
                    {
                        var nuevaImagenUrl = await _storageService.GuardarArchivoAsync(model.ImagenArchivo, "productos");
                        if (!string.IsNullOrEmpty(nuevaImagenUrl))
                        {
                            producto.ImagenUrl = nuevaImagenUrl;
                        }
                    }

                    producto.Nombre = model.Nombre;
                    producto.Descripcion = model.Descripcion;
                    producto.Precio = model.Precio;
                    producto.Stock = model.Stock;
                    producto.CodigoSku = model.CodigoSku;
                    producto.PesoKg = model.PesoKg;
                    producto.Activo = model.Activo;
                    producto.Destacado = model.Destacado;
                    producto.CategoriaId = model.CategoriaId;

                    _context.Update(producto);
                    await _context.SaveChangesAsync();

                    TempData["MensajeExito"] = $"Producto '{producto.Nombre}' actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            var categorias = await _context.Categorias.Where(c => c.Activo).OrderBy(c => c.Nombre).ToListAsync();
            model.CategoriasList = categorias.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre });
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole(TipoUsuario.Administrador.ToString());

            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id && (esAdmin || p.VendedorId == userId));
            if (producto == null)
                return Json(new { success = false, message = "Producto no encontrado o sin permisos." });

            // Desactivar lógicamente para no romper integridad de pedidos anteriores
            producto.Activo = false;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"El producto '{producto.Nombre}' fue dado de baja correctamente." });
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
