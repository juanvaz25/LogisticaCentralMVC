using LCP.Web.Models.Entities;
using LCP.Web.Models.Enums;
using LCP.Web.Models.ViewModels.Account;
using LCP.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LCP.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IStorageService _storageService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IStorageService storageService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _storageService = storageService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Correo electrónico o contraseña incorrectos.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains(TipoUsuario.Administrador.ToString()))
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl != "/" && !model.ReturnUrl.Contains("Home"))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Admin");
                }

                if (roles.Contains(TipoUsuario.Vendedor.ToString()))
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl != "/" && !model.ReturnUrl.Contains("Home"))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Vendedor");
                }

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "La cuenta ha sido bloqueada temporalmente por múltiples intentos fallidos.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Correo electrónico o contraseña incorrectos.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userExist = await _userManager.FindByEmailAsync(model.Email);
            if (userExist != null)
            {
                ModelState.AddModelError("Email", "El correo electrónico ya se encuentra registrado.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                PhoneNumber = model.PhoneNumber,
                Dni = model.Dni,
                Direccion = model.Direccion,
                Ciudad = model.Ciudad,
                Provincia = model.Provincia,
                NombreComercio = model.TipoUsuario == TipoUsuario.Vendedor ? model.NombreComercio : null,
                Cuit = model.TipoUsuario == TipoUsuario.Vendedor ? model.Cuit : null,
                EsVendedorActivo = model.TipoUsuario == TipoUsuario.Vendedor,
                FechaRegistro = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Asignar Rol
                var rolName = model.TipoUsuario == TipoUsuario.Vendedor 
                    ? TipoUsuario.Vendedor.ToString() 
                    : TipoUsuario.Comprador.ToString();

                if (!await _roleManager.RoleExistsAsync(rolName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(rolName));
                }

                await _userManager.AddToRoleAsync(user, rolName);

                // Auto login
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["MensajeExito"] = $"¡Bienvenido a Logística Centro País, {user.Nombre}!";

                if (model.TipoUsuario == TipoUsuario.Vendedor)
                    return RedirectToAction("Index", "Vendedor");

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            TempData["MensajeInfo"] = "Sesión cerrada correctamente.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new ProfileViewModel
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Dni = user.Dni,
                Direccion = user.Direccion,
                Ciudad = user.Ciudad,
                Provincia = user.Provincia,
                NombreComercio = user.NombreComercio,
                Cuit = user.Cuit,
                FotoPerfilUrl = user.FotoPerfilUrl,
                Rol = roles.FirstOrDefault() ?? "Usuario"
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            user.Nombre = model.Nombre;
            user.Apellido = model.Apellido;
            user.PhoneNumber = model.PhoneNumber;
            user.Dni = model.Dni;
            user.Direccion = model.Direccion;
            user.Ciudad = model.Ciudad;
            user.Provincia = model.Provincia;
            user.NombreComercio = model.NombreComercio;
            user.Cuit = model.Cuit;

            if (model.NuevaFoto != null && model.NuevaFoto.Length > 0)
            {
                var fotoUrl = await _storageService.GuardarArchivoAsync(model.NuevaFoto, "usuarios");
                if (!string.IsNullOrEmpty(fotoUrl))
                {
                    user.FotoPerfilUrl = fotoUrl;
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["MensajeExito"] = "Perfil actualizado con éxito.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["MensajeExito"] = "Contraseña modificada correctamente.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
