using LCP.Web.Data;
using LCP.Web.Models.Entities;
using LCP.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsuarios = await _context.Users.CountAsync();
            var totalVendedores = await _context.Users.CountAsync(u => u.EsVendedorActivo);
            var totalProductos = await _context.Productos.CountAsync(p => p.Activo);
            var totalPedidos = await _context.Pedidos.CountAsync();
            var totalIngresos = await _context.Pedidos.Where(p => p.Estado != EstadoPedido.Cancelado).SumAsync(p => p.Total);
            var totalNodos = await _context.NodosDistribucion.CountAsync(n => n.Activo);

            var ultimosPedidos = await _context.Pedidos
                .Include(p => p.Comprador)
                .Include(p => p.Vendedor)
                .OrderByDescending(p => p.FechaPedido)
                .Take(6)
                .ToListAsync();

            ViewBag.TotalUsuarios = totalUsuarios;
            ViewBag.TotalVendedores = totalVendedores;
            ViewBag.TotalProductos = totalProductos;
            ViewBag.TotalPedidos = totalPedidos;
            ViewBag.TotalIngresos = totalIngresos;
            ViewBag.TotalNodos = totalNodos;
            ViewBag.UltimosPedidos = ultimosPedidos;

            return View();
        }

        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _context.Users.OrderByDescending(u => u.FechaRegistro).ToListAsync();
            var listaConRoles = new List<(ApplicationUser Usuario, IList<string> Roles)>();

            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                listaConRoles.Add((u, roles));
            }

            return View(listaConRoles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVendedor(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Usuario no encontrado." });

            user.EsVendedorActivo = !user.EsVendedorActivo;
            await _userManager.UpdateAsync(user);

            var estado = user.EsVendedorActivo ? "habilitado" : "deshabilitado";
            return Json(new { success = true, message = $"El comercio/vendedor {user.NombreCompleto} ha sido {estado}.", nuevoEstado = user.EsVendedorActivo });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(string id, string nuevoRol)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Usuario no encontrado." });

            var rolesActuales = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, rolesActuales);

            if (!await _roleManager.RoleExistsAsync(nuevoRol))
            {
                await _roleManager.CreateAsync(new IdentityRole(nuevoRol));
            }

            await _userManager.AddToRoleAsync(user, nuevoRol);

            if (nuevoRol == TipoUsuario.Vendedor.ToString())
            {
                user.EsVendedorActivo = true;
                await _userManager.UpdateAsync(user);
            }

            return Json(new { success = true, message = $"Rol de {user.NombreCompleto} actualizado a '{nuevoRol}'." });
        }
    }
}
