using LCP.Web.Models.Entities;
using LCP.Web.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Asegurar que la base de datos esté creada
            await context.Database.MigrateAsync();

            // 1. Crear Roles si no existen
            string[] roles = { TipoUsuario.Administrador.ToString(), TipoUsuario.Vendedor.ToString(), TipoUsuario.Comprador.ToString() };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Crear Usuarios Iniciales
            // Admin
            var adminEmail = "admin@lcp.com.ar";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Nombre = "Juan",
                    Apellido = "Vázquez",
                    Dni = "38123456",
                    PhoneNumber = "2954-123456",
                    Direccion = "Av. San Martín 450",
                    Ciudad = "Santa Rosa",
                    Provincia = "La Pampa",
                    NombreComercio = "Logística Centro País HQ",
                    Cuit = "20-38123456-9",
                    EsVendedorActivo = true,
                    FechaRegistro = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, TipoUsuario.Administrador.ToString());
                }
            }

            // Vendedor 1
            var vendedor1Email = "vendedor1@lcp.com.ar";
            var vendedor1 = await userManager.FindByEmailAsync(vendedor1Email);
            if (vendedor1 == null)
            {
                vendedor1 = new ApplicationUser
                {
                    UserName = vendedor1Email,
                    Email = vendedor1Email,
                    EmailConfirmed = true,
                    Nombre = "Mariano",
                    Apellido = "López",
                    Dni = "32456789",
                    PhoneNumber = "2954-654321",
                    Direccion = "Parque Industrial Calle 2",
                    Ciudad = "Santa Rosa",
                    Provincia = "La Pampa",
                    NombreComercio = "Distribuidora Pampeana Central",
                    Cuit = "30-71234567-8",
                    EsVendedorActivo = true,
                    FechaRegistro = DateTime.UtcNow.AddMonths(-3)
                };
                var result = await userManager.CreateAsync(vendedor1, "Vendedor123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(vendedor1, TipoUsuario.Vendedor.ToString());
                }
            }

            // Vendedor 2
            var vendedor2Email = "vendedor2@lcp.com.ar";
            var vendedor2 = await userManager.FindByEmailAsync(vendedor2Email);
            if (vendedor2 == null)
            {
                vendedor2 = new ApplicationUser
                {
                    UserName = vendedor2Email,
                    Email = vendedor2Email,
                    EmailConfirmed = true,
                    Nombre = "Sofía",
                    Apellido = "Ramírez",
                    Dni = "35987654",
                    PhoneNumber = "2302-887766",
                    Direccion = "Ruta Prov. 1 Km 82",
                    Ciudad = "General Pico",
                    Provincia = "La Pampa",
                    NombreComercio = "AgroLogística Centro",
                    Cuit = "30-79876543-2",
                    EsVendedorActivo = true,
                    FechaRegistro = DateTime.UtcNow.AddMonths(-2)
                };
                var result = await userManager.CreateAsync(vendedor2, "Vendedor123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(vendedor2, TipoUsuario.Vendedor.ToString());
                }
            }

            // Comprador
            var compradorEmail = "comprador@lcp.com.ar";
            var comprador = await userManager.FindByEmailAsync(compradorEmail);
            if (comprador == null)
            {
                comprador = new ApplicationUser
                {
                    UserName = compradorEmail,
                    Email = compradorEmail,
                    EmailConfirmed = true,
                    Nombre = "Carlos",
                    Apellido = "Gómez",
                    Dni = "40112233",
                    PhoneNumber = "2954-778899",
                    Direccion = "Calle Quintana 280",
                    Ciudad = "Santa Rosa",
                    Provincia = "La Pampa",
                    EsVendedorActivo = false,
                    FechaRegistro = DateTime.UtcNow.AddMonths(-1)
                };
                var result = await userManager.CreateAsync(comprador, "Comprador123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(comprador, TipoUsuario.Comprador.ToString());
                }
            }

            // 3. Provincias y Nodos de Distribución
            if (!await context.Provincias.AnyAsync())
            {
                var provLaPampa = new Provincia { Nombre = "La Pampa", Codigo = "LP" };
                var provCordoba = new Provincia { Nombre = "Córdoba", Codigo = "CBA" };
                var provBuenosAires = new Provincia { Nombre = "Buenos Aires", Codigo = "BA" };
                var provSanLuis = new Provincia { Nombre = "San Luis", Codigo = "SL" };

                context.Provincias.AddRange(provLaPampa, provCordoba, provBuenosAires, provSanLuis);
                await context.SaveChangesAsync();

                var nodos = new List<NodoDistribucion>
                {
                    new NodoDistribucion { Nombre = "Nodo Central Santa Rosa - LCP Hub", Direccion = "Av. Spinetto 1250", Ciudad = "Santa Rosa", CodigoPostal = "6300", Telefono = "2954-411200", TiempoEstimadoHoras = 12, TarifaBase = 2200m, ProvinciaId = provLaPampa.Id },
                    new NodoDistribucion { Nombre = "Nodo General Pico - Centro Norte", Direccion = "Calle 9 N° 1420", Ciudad = "General Pico", CodigoPostal = "6360", Telefono = "2302-421100", TiempoEstimadoHoras = 24, TarifaBase = 2800m, ProvinciaId = provLaPampa.Id },
                    new NodoDistribucion { Nombre = "Nodo Toay - Centro Metropolitano", Direccion = "Bv. Brown 320", Ciudad = "Toay", CodigoPostal = "6303", Telefono = "2954-492010", TiempoEstimadoHoras = 18, TarifaBase = 2400m, ProvinciaId = provLaPampa.Id },
                    new NodoDistribucion { Nombre = "Nodo Realicó - Cruce Nacional 35/188", Direccion = "Acceso Balbín 800", Ciudad = "Realicó", CodigoPostal = "6200", Telefono = "2302-461320", TiempoEstimadoHoras = 24, TarifaBase = 3200m, ProvinciaId = provLaPampa.Id },
                    new NodoDistribucion { Nombre = "Nodo Río Cuarto - Enlace Sur", Direccion = "Ruta Nac. A005 Km 4", Ciudad = "Río Cuarto", CodigoPostal = "5800", Telefono = "358-4645000", TiempoEstimadoHoras = 36, TarifaBase = 4500m, ProvinciaId = provCordoba.Id },
                    new NodoDistribucion { Nombre = "Nodo Bahía Blanca - Puerto y Enlace", Direccion = "Av. Colón 2100", Ciudad = "Bahía Blanca", CodigoPostal = "8000", Telefono = "291-4567800", TiempoEstimadoHoras = 36, TarifaBase = 4800m, ProvinciaId = provBuenosAires.Id },
                    new NodoDistribucion { Nombre = "Nodo Villa Mercedes / San Luis", Direccion = "Ruta Nac. 7 Km 690", Ciudad = "Villa Mercedes", CodigoPostal = "5730", Telefono = "2657-423100", TiempoEstimadoHoras = 40, TarifaBase = 4900m, ProvinciaId = provSanLuis.Id }
                };

                context.NodosDistribucion.AddRange(nodos);
                await context.SaveChangesAsync();
            }

            // 4. Categorías
            if (!await context.Categorias.AnyAsync())
            {
                var categorias = new List<Categoria>
                {
                    new Categoria { Nombre = "Alimentos y Bebidas Regionales", Descripcion = "Productos pampeanos, miel, chacinados, quesos, conservas y vinos del centro del país", Icono = "fas fa-utensils" },
                    new Categoria { Nombre = "Insumos y Agroindustria", Descripcion = "Semillas, fertilizantes, alimentos balanceados, boyeros y elementos de campo", Icono = "fas fa-seedling" },
                    new Categoria { Nombre = "Herramientas y Maquinaria", Descripcion = "Equipamiento para taller, herramientas manuales y eléctricas para trabajo pesado", Icono = "fas fa-tools" },
                    new Categoria { Nombre = "Repuestos y Autopartes", Descripcion = "Filtros, correas, lubricantes y repuestos automotores y para transporte de carga", Icono = "fas fa-cogs" },
                    new Categoria { Nombre = "Indumentaria y Seguridad Laboral", Descripcion = "Calzado de seguridad, ropa de trabajo, guantes, cascos y protección industrial", Icono = "fas fa-vest" },
                    new Categoria { Nombre = "Tecnología y Logística", Descripcion = "Lectores de código de barras, balanzas industriales, etiquetas y hardware", Icono = "fas fa-laptop" }
                };

                context.Categorias.AddRange(categorias);
                await context.SaveChangesAsync();
            }

            // 5. Productos
            if (!await context.Productos.AnyAsync())
            {
                var catAlimentos = await context.Categorias.FirstOrDefaultAsync(c => c.Nombre.Contains("Alimentos"));
                var catAgro = await context.Categorias.FirstOrDefaultAsync(c => c.Nombre.Contains("Agroindustria"));
                var catHerramientas = await context.Categorias.FirstOrDefaultAsync(c => c.Nombre.Contains("Herramientas"));
                var catRepuestos = await context.Categorias.FirstOrDefaultAsync(c => c.Nombre.Contains("Repuestos"));
                var catIndumentaria = await context.Categorias.FirstOrDefaultAsync(c => c.Nombre.Contains("Indumentaria"));
                var catTecno = await context.Categorias.FirstOrDefaultAsync(c => c.Nombre.Contains("Tecnología"));

                var v1 = await userManager.FindByEmailAsync(vendedor1Email);
                var v2 = await userManager.FindByEmailAsync(vendedor2Email);

                if (v1 != null && v2 != null)
                {
                    var productos = new List<Producto>
                    {
                        new Producto
                        {
                            Nombre = "Miel Pura Pampeana de Monte (Caja x 12 Frascos 1kg)",
                            Descripcion = "Miel multifloral 100% pura cosechada en el monte pampeano. Calidad de exportación con certificación provincial. Envasada en origen.",
                            Precio = 48000m,
                            Stock = 45,
                            ImagenUrl = "https://images.unsplash.com/photo-1587049352846-4a222e784d38?w=500&auto=format&fit=crop&q=60",
                            CodigoSku = "MIEL-LP-01",
                            PesoKg = 13.5m,
                            Destacado = true,
                            CategoriaId = catAlimentos?.Id ?? 1,
                            VendedorId = v1.Id,
                            FechaCreacion = DateTime.UtcNow.AddDays(-20)
                        },
                        new Producto
                        {
                            Nombre = "Queso Gouda Artesanal Horma Entera (4.5 kg)",
                            Descripcion = "Queso Gouda de pasta semidura elaborado con leche seleccionada de tambos pampeanos. Estacionamiento mínimo de 60 días.",
                            Precio = 36500m,
                            Stock = 20,
                            ImagenUrl = "https://images.unsplash.com/photo-1486297678162-eb2a19b0a32d?w=500&auto=format&fit=crop&q=60",
                            CodigoSku = "QUESO-GOUDA-01",
                            PesoKg = 4.8m,
                            Destacado = true,
                            CategoriaId = catAlimentos?.Id ?? 1,
                            VendedorId = v1.Id,
                            FechaCreacion = DateTime.UtcNow.AddDays(-15)
                        },
                        new Producto
                        {
                            Nombre = "Electrificador Rural / Boyero Solar 120 km Dual",
                            Descripcion = "Boyero electrificador solar de alto impacto para contención de hacienda. Panel monocristalino integrado y batería de gel de libre mantenimiento.",
                            Precio = 185000m,
                            Stock = 12,
                            ImagenUrl = "https://images.unsplash.com/photo-1581092160607-ee22621dd758?w=500&auto=format&fit=crop&q=60",
                            CodigoSku = "BOY-SOLAR-120",
                            PesoKg = 8.2m,
                            Destacado = true,
                            CategoriaId = catAgro?.Id ?? 2,
                            VendedorId = v2.Id,
                            FechaCreacion = DateTime.UtcNow.AddDays(-18)
                        },
                        new Producto
                        {
                            Nombre = "Alambre de Alta Resistencia San Martín 17/15 (Rollo 1000m)",
                            Descripcion = "Rollo de alambre galvanizado de alta resistencia especial para alambrados perimetrales e internos en establecimientos rurales.",
                            Precio = 245000m,
                            Stock = 30,
                            ImagenUrl = "https://images.unsplash.com/photo-1504917599217-d4dc5ebe6122?w=500&auto=format&fit=crop&q=60",
                            CodigoSku = "ALAMB-1715-1K",
                            PesoKg = 42.0m,
                            Destacado = false,
                            CategoriaId = catAgro?.Id ?? 2,
                            VendedorId = v2.Id,
                            FechaCreacion = DateTime.UtcNow.AddDays(-10)
                        },
                        new Producto
                        {
                            Nombre = "Juego de Llaves Combinadas Cromo Vanadio (12 piezas)",
                            Descripcion = "Set profesional de llaves combinadas métricas de 6 a 22 mm en estuche organizador de lona reforzada. Tratamiento anticorrosión.",
                            Precio = 52000m,
                            Stock = 25,
                            ImagenUrl = "https://images.unsplash.com/photo-1530124566582-a618bc2615dc?w=500&auto=format&fit=crop&q=60",
                            CodigoSku = "LLAV-COMB-12P",
                            PesoKg = 3.1m,
                            Destacado = true,
                            CategoriaId = catHerramientas?.Id ?? 3,
                            VendedorId = v1.Id,
                            FechaCreacion = DateTime.UtcNow.AddDays(-8)
                        },
                        new Producto
                        {
                            Nombre = "Botines de Seguridad con Puntera de Acero Dieléctricos",
                            Descripcion = "Calzado de seguridad industrial certificado con suela de poliuretano bidensidad resistente a hidrocarburos y plantilla confort.",
                            Precio = 68000m,
                            Stock = 50,
                            ImagenUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=500&auto=format&fit=crop&q=60",
                            CodigoSku = "BOT-SEG-PUNT",
                            PesoKg = 2.0m,
                            Destacado = true,
                            CategoriaId = catIndumentaria?.Id ?? 5,
                            VendedorId = v2.Id,
                            FechaCreacion = DateTime.UtcNow.AddDays(-5)
                        },
                        new Producto
                        {
                            Nombre = "Lector de Código de Barras y QR Inalámbrico Industrial",
                            Descripcion = "Escáner 2D omnidireccional resistente a caídas de hasta 2 metros con base de carga y transmisión Bluetooth de largo alcance para logística.",
                            Precio = 94000m,
                            Stock = 18,
                            ImagenUrl = "https://images.unsplash.com/photo-1526738549149-8e07eca6c147?w=500&auto=format&fit=crop&q=60",
                            CodigoSku = "SCAN-QR-2D",
                            PesoKg = 0.8m,
                            Destacado = false,
                            CategoriaId = catTecno?.Id ?? 6,
                            VendedorId = v1.Id,
                            FechaCreacion = DateTime.UtcNow.AddDays(-3)
                        },
                        new Producto
                        {
                            Nombre = "Kit Filtros y Aceite Sintético para Camiones y Utilitarios",
                            Descripcion = "Combo de mantenimiento que incluye filtro de aire, filtro de combustible, filtro de aceite y bidón 20L de lubricante sintético 15W40.",
                            Precio = 138000m,
                            Stock = 15,
                            ImagenUrl = "https://images.unsplash.com/photo-1486006920555-c77dce18193b?w=500&auto=format&fit=crop&q=60",
                            CodigoSku = "KIT-LUB-15W40",
                            PesoKg = 22.0m,
                            Destacado = false,
                            CategoriaId = catRepuestos?.Id ?? 4,
                            VendedorId = v2.Id,
                            FechaCreacion = DateTime.UtcNow.AddDays(-2)
                        }
                    };

                    context.Productos.AddRange(productos);
                    await context.SaveChangesAsync();
                }
            }

            // 6. Pedidos Iniciales de Prueba
            if (!await context.Pedidos.AnyAsync())
            {
                var compradorUser = await userManager.FindByEmailAsync(compradorEmail);
                var vendedor1User = await userManager.FindByEmailAsync(vendedor1Email);
                var prodMiel = await context.Productos.FirstOrDefaultAsync(p => p.CodigoSku == "MIEL-LP-01");
                var prodQueso = await context.Productos.FirstOrDefaultAsync(p => p.CodigoSku == "QUESO-GOUDA-01");
                var nodoCentral = await context.NodosDistribucion.FirstOrDefaultAsync();

                if (compradorUser != null && vendedor1User != null && prodMiel != null && prodQueso != null)
                {
                    // Pedido 1: En Camino con Historial y QR
                    var tracking1 = "LCP-20260918-7821";
                    var tokenEntrega1 = Guid.NewGuid().ToString("N")[..12].ToUpper();
                    var pedido1 = new Pedido
                    {
                        CodigoSeguimiento = tracking1,
                        FechaPedido = DateTime.UtcNow.AddDays(-2),
                        FechaEntregaEstimada = DateTime.UtcNow.AddDays(1),
                        Estado = EstadoPedido.EnCamino,
                        Subtotal = prodMiel.Precio * 1 + prodQueso.Precio * 2,
                        CostoEnvio = 2200m,
                        Total = (prodMiel.Precio * 1 + prodQueso.Precio * 2) + 2200m,
                        MetodoPago = MetodoPago.Transferencia,
                        DireccionEntrega = "Calle Quintana 280",
                        CiudadEntrega = "Santa Rosa",
                        ProvinciaEntrega = "La Pampa",
                        CodigoPostalEntrega = "6300",
                        TelefonoContacto = "2954-778899",
                        NotasAdicionales = "Tocar timbre casa de rejas blancas",
                        TokenEntregaQr = tokenEntrega1,
                        NodoDistribucionId = nodoCentral?.Id,
                        CompradorId = compradorUser.Id,
                        VendedorId = vendedor1User.Id
                    };

                    pedido1.Detalles.Add(new DetallePedido
                    {
                        ProductoId = prodMiel.Id,
                        Cantidad = 1,
                        PrecioUnitario = prodMiel.Precio,
                        Subtotal = prodMiel.Precio * 1
                    });
                    pedido1.Detalles.Add(new DetallePedido
                    {
                        ProductoId = prodQueso.Id,
                        Cantidad = 2,
                        PrecioUnitario = prodQueso.Precio,
                        Subtotal = prodQueso.Precio * 2
                    });

                    pedido1.Historial.Add(new HistorialSeguimiento
                    {
                        Fecha = DateTime.UtcNow.AddDays(-2),
                        Ubicacion = "Distribuidora Pampeana Central - Santa Rosa",
                        Descripcion = "Pedido recibido y registrado en el sistema LCP",
                        Estado = EstadoPedido.EnProceso
                    });
                    pedido1.Historial.Add(new HistorialSeguimiento
                    {
                        Fecha = DateTime.UtcNow.AddDays(-1).AddHours(4),
                        Ubicacion = "Distribuidora Pampeana Central - Empaque",
                        Descripcion = "Mercadería embalada y preparada para despacho logístico",
                        Estado = EstadoPedido.EnPreparacion
                    });
                    pedido1.Historial.Add(new HistorialSeguimiento
                    {
                        Fecha = DateTime.UtcNow.AddHours(-6),
                        Ubicacion = "Nodo Central Santa Rosa - LCP Hub",
                        Descripcion = "En tránsito hacia el domicilio de entrega asignado",
                        Estado = EstadoPedido.EnCamino
                    });

                    // Pedido 2: Entregado
                    var tracking2 = "LCP-20260910-3419";
                    var tokenEntrega2 = Guid.NewGuid().ToString("N")[..12].ToUpper();
                    var pedido2 = new Pedido
                    {
                        CodigoSeguimiento = tracking2,
                        FechaPedido = DateTime.UtcNow.AddDays(-8),
                        FechaEntregaEstimada = DateTime.UtcNow.AddDays(-6),
                        FechaEntregaReal = DateTime.UtcNow.AddDays(-6),
                        Estado = EstadoPedido.Entregado,
                        Subtotal = prodMiel.Precio * 2,
                        CostoEnvio = 2200m,
                        Total = (prodMiel.Precio * 2) + 2200m,
                        MetodoPago = MetodoPago.Tarjeta,
                        DireccionEntrega = "Av. San Martín 850",
                        CiudadEntrega = "Santa Rosa",
                        ProvinciaEntrega = "La Pampa",
                        CodigoPostalEntrega = "6300",
                        TelefonoContacto = "2954-778899",
                        TokenEntregaQr = tokenEntrega2,
                        NodoDistribucionId = nodoCentral?.Id,
                        CompradorId = compradorUser.Id,
                        VendedorId = vendedor1User.Id
                    };

                    pedido2.Detalles.Add(new DetallePedido
                    {
                        ProductoId = prodMiel.Id,
                        Cantidad = 2,
                        PrecioUnitario = prodMiel.Precio,
                        Subtotal = prodMiel.Precio * 2
                    });

                    pedido2.Historial.Add(new HistorialSeguimiento
                    {
                        Fecha = DateTime.UtcNow.AddDays(-8),
                        Ubicacion = "Santa Rosa",
                        Descripcion = "Pedido generado y abonado",
                        Estado = EstadoPedido.EnProceso
                    });
                    pedido2.Historial.Add(new HistorialSeguimiento
                    {
                        Fecha = DateTime.UtcNow.AddDays(-7),
                        Ubicacion = "Nodo Central Santa Rosa",
                        Descripcion = "Despachado en unidad de reparto LCP",
                        Estado = EstadoPedido.EnCamino
                    });
                    pedido2.Historial.Add(new HistorialSeguimiento
                    {
                        Fecha = DateTime.UtcNow.AddDays(-6),
                        Ubicacion = "Domicilio Comprador",
                        Descripcion = "Envío entregado satisfactoriamente con validación QR",
                        Estado = EstadoPedido.Entregado
                    });

                    context.Pedidos.AddRange(pedido1, pedido2);
                    await context.SaveChangesAsync();
                }
            }

            // 7. Consultas Asíncronas de Ejemplo
            if (!await context.ConsultasSoporte.AnyAsync())
            {
                var comp = await userManager.FindByEmailAsync(compradorEmail);
                var vend = await userManager.FindByEmailAsync(vendedor1Email);
                var prod = await context.Productos.FirstOrDefaultAsync(p => p.CodigoSku == "MIEL-LP-01");

                if (comp != null && vend != null && prod != null)
                {
                    var consulta1 = new ConsultaSoporte
                    {
                        Asunto = "Consulta sobre fecha de vencimiento y lote de la miel",
                        Mensaje = "Hola, buenas tardes. Quería consultar qué fecha de vencimiento tiene el lote actual de la caja de 12 frascos de miel. Muchas gracias.",
                        FechaEnvio = DateTime.UtcNow.AddDays(-4),
                        Respondido = true,
                        Respuesta = "Hola Carlos, el lote actual fue envasado hace 15 días y cuenta con vencimiento a 2 años (Agosto 2028). Tenemos stock disponible inmediato.",
                        FechaRespuesta = DateTime.UtcNow.AddDays(-4).AddHours(2),
                        EmisorId = comp.Id,
                        ReceptorId = vend.Id,
                        ProductoId = prod.Id
                    };

                    var consulta2 = new ConsultaSoporte
                    {
                        Asunto = "Consulta a Soporte Técnico LCP: Tiempos de entrega a General Acha",
                        Mensaje = "Buenas tardes equipo de LCP, ¿cuál es la frecuencia de viajes del nodo Santa Rosa hacia General Acha para cargas pesadas?",
                        FechaEnvio = DateTime.UtcNow.AddHours(-5),
                        Respondido = false,
                        EmisorId = comp.Id,
                        ReceptorId = null, // Para administradores/soporte general
                        ProductoId = null
                    };

                    context.ConsultasSoporte.AddRange(consulta1, consulta2);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
