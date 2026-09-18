using LCP.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Controllers.Api
{
    [Route("api/v1/productos")]
    [ApiController]
    public class ApiProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/v1/productos
        [HttpGet]
        public async Task<IActionResult> GetProductos([FromQuery] int? categoriaId, [FromQuery] string? q)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Vendedor)
                .Where(p => p.Activo);

            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                query = query.Where(p => p.Nombre.ToLower().Contains(term) || p.Descripcion.ToLower().Contains(term));
            }

            var productos = await query
                .OrderByDescending(p => p.FechaCreacion)
                .Take(50)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Descripcion,
                    p.Precio,
                    p.Stock,
                    p.ImagenUrl,
                    p.CodigoSku,
                    p.PesoKg,
                    Categoria = p.Categoria != null ? p.Categoria.Nombre : "",
                    Vendedor = p.Vendedor != null ? (p.Vendedor.NombreComercio ?? p.Vendedor.NombreCompleto) : "",
                    p.FechaCreacion
                })
                .ToListAsync();

            return Ok(new { total = productos.Count, items = productos });
        }

        // GET: api/v1/productos/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductoPorId(int id)
        {
            var p = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Vendedor)
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

            if (p == null)
                return NotFound(new { error = $"Producto con Id {id} no encontrado." });

            return Ok(new
            {
                p.Id,
                p.Nombre,
                p.Descripcion,
                p.Precio,
                p.Stock,
                p.ImagenUrl,
                p.CodigoSku,
                p.PesoKg,
                p.Destacado,
                Categoria = p.Categoria != null ? p.Categoria.Nombre : "",
                Vendedor = p.Vendedor != null ? (p.Vendedor.NombreComercio ?? p.Vendedor.NombreCompleto) : "",
                p.FechaCreacion
            });
        }
    }
}
