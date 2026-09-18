using LCP.Web.Data;
using LCP.Web.Models.Entities;
using LCP.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Cadena de conexión y DbContext con SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configuración de ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Opciones de Contraseña
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Bloqueo y Usuario
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Configuración de Cookies de Autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// 4. Configuración de Sesión (para Carrito de Compras)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 5. Inyección de Dependencias para Servicios de la Aplicación
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IQRService, QRService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IEstadisticasService, EstadisticasService>();

// 6. Configuración de MVC y Controladores API
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilationIfAvailable();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// 7. Pipeline de Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// 8. Rutas de Controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers(); // Para controladores API REST

// 9. Inicialización de Base de Datos, Data Seeding y Stored Procedures
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var estadisticasService = services.GetRequiredService<IEstadisticasService>();

        // Ejecutar migraciones y Data Seeding
        await DbInitializer.InitializeAsync(context, userManager, roleManager);

        // Crear o actualizar Stored Procedures
        await estadisticasService.CrearStoredProceduresSiNoExistenAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos de LCP.");
    }
}

app.Run();

// Extensión para Razor Runtime opcional
public static class ServiceExtensions
{
    public static IMvcBuilder AddRazorRuntimeCompilationIfAvailable(this IMvcBuilder builder)
    {
        return builder;
    }
}
