using LCP.Web.Data;
using LCP.Web.Models.ViewModels.Productos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(CatalogoFiltroViewModel filtro)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Vendedor)
                .Where(p => p.Activo);

            // Búsqueda por texto (nombre, descripción, sku)
            if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
            {
                var term = filtro.Busqueda.Trim().ToLower();
                query = query.Where(p => p.Nombre.ToLower().Contains(term) || 
                                         p.Descripcion.ToLower().Contains(term) ||
                                         (p.CodigoSku != null && p.CodigoSku.ToLower().Contains(term)));
            }

            // Filtro por categoría
            if (filtro.CategoriaId.HasValue && filtro.CategoriaId.Value > 0)
            {
                query = query.Where(p => p.CategoriaId == filtro.CategoriaId.Value);
            }

            // Filtro por rango de precio
            if (filtro.PrecioMin.HasValue && filtro.PrecioMin.Value > 0)
            {
                query = query.Where(p => p.Precio >= filtro.PrecioMin.Value);
            }
            if (filtro.PrecioMax.HasValue && filtro.PrecioMax.Value > 0)
            {
                query = query.Where(p => p.Precio <= filtro.PrecioMax.Value);
            }

            // Ordenamiento
            query = filtro.OrdenarPor switch
            {
                "precio_asc" => query.OrderBy(p => p.Precio),
                "precio_desc" => query.OrderByDescending(p => p.Precio),
                "nombre" => query.OrderBy(p => p.Nombre),
                _ => query.OrderByDescending(p => p.FechaCreacion)
            };

            filtro.TotalElementos = await query.CountAsync();
            filtro.Pagina = Math.Max(1, filtro.Pagina);
            filtro.ElementosPorPagina = 8;

            filtro.Productos = await query
                .Skip((filtro.Pagina - 1) * filtro.ElementosPorPagina)
                .Take(filtro.ElementosPorPagina)
                .ToListAsync();

            filtro.Categorias = await _context.Categorias
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(filtro);
        }

        [HttpGet]
        public async Task<IActionResult> BuscarSensitivo(string? q, int? categoriaId, decimal? precioMin, decimal? precioMax)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                query = query.Where(p => p.Nombre.ToLower().Contains(term) ||
                                         p.Descripcion.ToLower().Contains(term) ||
                                         (p.CodigoSku != null && p.CodigoSku.ToLower().Contains(term)));
            }

            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

            if (precioMin.HasValue)
                query = query.Where(p => p.Precio >= precioMin.Value);

            if (precioMax.HasValue)
                query = query.Where(p => p.Precio <= precioMax.Value);

            var resultados = await query
                .Take(10)
                .Select(p => new
                {
                    id = p.Id,
                    nombre = p.Nombre,
                    precio = p.Precio,
                    imagenUrl = p.ImagenUrl ?? "/images/no-image.png",
                    categoria = p.Categoria != null ? p.Categoria.Nombre : "",
                    stock = p.Stock
                })
                .ToListAsync();

            return Json(resultados);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Vendedor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
                return NotFound();

            // Incrementar visitas
            producto.NumeroVisitas++;
            await _context.SaveChangesAsync();

            // Productos relacionados
            var relacionados = await _context.Productos
                .Where(p => p.CategoriaId == producto.CategoriaId && p.Id != producto.Id && p.Activo)
                .Take(4)
                .ToListAsync();

            ViewBag.Relacionados = relacionados;

            return View(producto);
        }
    }
}
