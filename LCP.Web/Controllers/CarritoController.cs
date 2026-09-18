using System.Text.Json;
using LCP.Web.Data;
using LCP.Web.Models.ViewModels.Pedidos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string SessionCartKey = "LCP_CART_SESSION";

        public CarritoController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<CarritoItemViewModel> ObtenerCarritoDeSesion()
        {
            var cartJson = HttpContext.Session.GetString(SessionCartKey);
            if (string.IsNullOrEmpty(cartJson))
                return new List<CarritoItemViewModel>();

            return JsonSerializer.Deserialize<List<CarritoItemViewModel>>(cartJson) ?? new List<CarritoItemViewModel>();
        }

        private void GuardarCarritoEnSesion(List<CarritoItemViewModel> items)
        {
            var cartJson = JsonSerializer.Serialize(items);
            HttpContext.Session.SetString(SessionCartKey, cartJson);
        }

        public IActionResult Index()
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

            var items = ObtenerCarritoDeSesion();
            var model = new CarritoViewModel
            {
                Items = items
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Agregar(int productoId, int cantidad = 1)
        {
            if (User.IsInRole("Vendedor") || User.IsInRole("Administrador"))
            {
                return Json(new { success = false, message = "Tu perfil actual es de gestión (Vendedor / Administrador). Para realizar compras debes usar una cuenta de Comprador." });
            }

            if (cantidad <= 0) cantidad = 1;

            var producto = await _context.Productos
                .Include(p => p.Vendedor)
                .FirstOrDefaultAsync(p => p.Id == productoId && p.Activo);

            if (producto == null)
            {
                return Json(new { success = false, message = "El producto no está disponible o no existe." });
            }

            var items = ObtenerCarritoDeSesion();
            var itemExistente = items.FirstOrDefault(i => i.ProductoId == productoId);

            if (itemExistente != null)
            {
                var nuevaCantidad = itemExistente.Cantidad + cantidad;
                if (nuevaCantidad > producto.Stock)
                {
                    return Json(new { success = false, message = $"Stock insuficiente. Máximo disponible: {producto.Stock} unidades." });
                }
                itemExistente.Cantidad = nuevaCantidad;
            }
            else
            {
                if (cantidad > producto.Stock)
                {
                    return Json(new { success = false, message = $"Stock insuficiente. Máximo disponible: {producto.Stock} unidades." });
                }

                items.Add(new CarritoItemViewModel
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = cantidad,
                    ImagenUrl = producto.ImagenUrl,
                    VendedorId = producto.VendedorId,
                    NombreVendedor = producto.Vendedor?.NombreComercio ?? producto.Vendedor?.NombreCompleto
                });
            }

            GuardarCarritoEnSesion(items);
            var totalItems = items.Sum(i => i.Cantidad);

            return Json(new { success = true, message = "Producto agregado al carrito de compras.", totalItems });
        }

        [HttpPost]
        public IActionResult ActualizarCantidad(int productoId, int cantidad)
        {
            var items = ObtenerCarritoDeSesion();
            var item = items.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
            {
                if (cantidad <= 0)
                {
                    items.Remove(item);
                }
                else
                {
                    item.Cantidad = cantidad;
                }
                GuardarCarritoEnSesion(items);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Eliminar(int productoId)
        {
            var items = ObtenerCarritoDeSesion();
            var item = items.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
            {
                items.Remove(item);
                GuardarCarritoEnSesion(items);
                TempData["MensajeInfo"] = "Producto eliminado del carrito.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Limpiar()
        {
            HttpContext.Session.Remove(SessionCartKey);
            TempData["MensajeInfo"] = "El carrito ha sido vaciado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Contador()
        {
            var items = ObtenerCarritoDeSesion();
            var count = items.Sum(i => i.Cantidad);
            return Json(new { count });
        }
    }
}
