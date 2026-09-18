using System.Diagnostics;
using LCP.Web.Data;
using LCP.Web.Models;
using LCP.Web.Models.ViewModels.Pedidos;
using LCP.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPedidoService _pedidoService;

        public HomeController(ApplicationDbContext context, IPedidoService pedidoService)
        {
            _context = context;
            _pedidoService = pedidoService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Administrador"))
                    return RedirectToAction("Index", "Admin");
                if (User.IsInRole("Vendedor"))
                    return RedirectToAction("Index", "Vendedor");
            }

            var productosDestacados = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Vendedor)
                .Where(p => p.Activo && p.Stock > 0 && p.Destacado)
                .OrderByDescending(p => p.FechaCreacion)
                .Take(8)
                .ToListAsync();

            var categorias = await _context.Categorias
                .Include(c => c.Productos)
                .Where(c => c.Activo)
                .ToListAsync();

            var totalEnvios = await _context.Pedidos.CountAsync(p => p.Estado == Models.Enums.EstadoPedido.Entregado);
            var totalNodos = await _context.NodosDistribucion.CountAsync(n => n.Activo);
            var totalVendedores = await _context.Users.CountAsync(u => u.EsVendedorActivo);

            ViewBag.ProductosDestacados = productosDestacados;
            ViewBag.Categorias = categorias;
            ViewBag.TotalEnvios = totalEnvios > 0 ? totalEnvios + 150 : 150;
            ViewBag.TotalNodos = totalNodos;
            ViewBag.TotalVendedores = totalVendedores > 0 ? totalVendedores + 10 : 12;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Seguimiento(string? codigo)
        {
            var model = new SeguimientoViewModel
            {
                CodigoBusqueda = codigo
            };

            if (!string.IsNullOrWhiteSpace(codigo))
            {
                var pedido = await _pedidoService.ObtenerPorCodigoTrackingAsync(codigo);
                if (pedido != null)
                {
                    model.Pedido = pedido;
                    model.Encontrado = true;
                }
                else
                {
                    model.Encontrado = false;
                    model.MensajeError = $"No se encontró ningún envío asociado al código de seguimiento '{codigo}'. Verifique e intente nuevamente.";
                }
            }

            return View(model);
        }

        public async Task<IActionResult> NodosPublicos()
        {
            var nodos = await _context.NodosDistribucion
                .Include(n => n.Provincia)
                .Where(n => n.Activo)
                .OrderBy(n => n.Provincia!.Nombre)
                .ThenBy(n => n.Nombre)
                .ToListAsync();

            return View(nodos);
        }

        public IActionResult Contacto()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
