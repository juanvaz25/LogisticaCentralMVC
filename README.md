# Logística Centro País (LCP) - Sistema Web Integral

Sistema integral de comercio electrónico, gestión de producción regional y logística con trazabilidad en tiempo real y confirmación de entrega por código QR, desarrollado sobre .NET 10 bajo el patrón de arquitectura ASP.NET Core MVC con Entity Framework Core (Code First) y SQL Server LocalDB.

---

## 1. Descripción del Proyecto

Logística Centro País (LCP) surge de la propuesta de articular un centro logístico y nodo distribuidor en la capital pampeana (Santa Rosa, La Pampa), aprovechando su ubicación geográfica estratégica en el centro de Argentina.

La plataforma conecta productores, distribuidoras y comercios de la región central (La Pampa, Córdoba, Buenos Aires, San Luis) con compradores finales, centralizando la venta, el despacho logístico a través de nodos de distribución interconectados y el seguimiento punto a punto de cada envío.

### Funcionalidades por Perfil de Usuario

- **Comprador**:
  - Exploración del catálogo de publicaciones con búsqueda sensitiva en vivo y filtros por categoría.
  - Carrito de compras en sesión con control de stock en tiempo real.
  - Proceso de checkout con selección de provincia y carga dinámica de nodos logísticos mediante dropdowns anidados.
  - Generación automática de comprobante, código de seguimiento único (ej. `LCP-20260918-7821`) y código QR criptográfico para validación de entrega.
  - Consulta y seguimiento del estado del envío en una línea de tiempo interactiva con 5 estados.
  - Sistema de mensajería asíncrona para enviar consultas dirigidas al soporte central o a los comercios a los que les compró.

- **Vendedor / Comercio**:
  - Panel de control (Dashboard) con KPIs de facturación, pedidos y publicaciones procesados mediante Stored Procedures.
  - Gestión completa de productos (CRUD con subida de imágenes a servidor y validación de SKU y stock).
  - Gestión de ventas y actualización del estado del pedido (`En proceso`, `En preparación`, `En camino`, `Entregado`, `Cancelado`).
  - Atención y respuesta a consultas enviadas por clientes directamente a su comercio.
  - Restricción comercial: interfaz exclusiva para gestión y venta, sin acceso a funciones de compra.

- **Administrador**:
  - Dashboard global con métricas consolidadas del sistema y facturación total.
  - Gestión integral de usuarios (activación/desactivación de cuentas y reasignación de roles Identity).
  - Administración de Nodos de Distribución (tarifas base, tiempos estimados, direcciones y horarios de atención).
  - Administración de Categorías y Rubros comerciales.
  - Control global de publicaciones, pedidos y resolución de consultas dirigidas al soporte central.
  - Visualización de reportes ejecutivos alimentados por Stored Procedures de SQL Server.

---

## 2. Requerimientos de la Guía y Arquitectura

| Requerimiento | Implementación en LCP |
|---|---|
| **Plataforma y Framework** | .NET 10 (`net10.0`) con ASP.NET Core MVC. |
| **Acceso a Datos y ORM** | Entity Framework Core (Code First) con SQL Server LocalDB. |
| **Inyección de Dependencias** | Registro formal de servicios en Program.cs (`IPedidoService`, `IEstadisticasService`, `IQRService`, `IStorageService`). |
| **Herramientas Front-End** | SweetAlert2 para alertas interactivas, DataTables en español con paginación y ordenamiento, Bootstrap 5 y FontAwesome 6. |
| **Modelo de Dominio (6+ entidades)** | 8 entidades principales con DataAnnotations y Fluent API: `ApplicationUser`, `Categoria`, `Provincia`, `NodoDistribucion`, `Producto`, `Pedido`, `DetallePedido`, `HistorialSeguimiento`, `ConsultaSoporte`. |
| **Migraciones EF Core (4+ sucesivas)** | 4 migraciones secuenciales documentadas: `01_InitialIdentityAndModels`, `02_AddProductVisitsAndLogisticsHours`, `03_AddOrderRatingAndAuditFields`, `04_AddSupportUrgencyAndLogisticsOperator`. |
| **Data Seeding** | Carga inicial automática de usuarios por rol, provincias, nodos, categorías, productos y pedidos de prueba en `DbInitializer.cs`. |
| **Vistas y Componentes** | Shared `_Layout.cshtml`, ViewModels independientes, Tag Helpers, Dropdowns anidados vía AJAX, y ViewComponents (`CarritoBadge`, `TrackingTimeline`). |
| **Stored Procedures** | 2 procedimientos almacenados en SQL Server: `sp_ObtenerEstadisticasVendedor` y `sp_ListarPedidosPorEstado`. |
| **Seguridad y Roles** | ASP.NET Core Identity con 3 roles estrictos: `Administrador`, `Vendedor` y `Comprador`. |
| **Servicios Extra** | API REST (`/api/v1/productos`, `/api/v1/tracking/{codigo}`), subida física de archivos de imagen, y generación de códigos QR con QRCoder. |

---

## 3. Modelo de Entidades y Base de Datos

1. **ApplicationUser**: Hereda de `IdentityUser`, incluye Nombre, Apellido, DNI, CUIT, Nombre de Comercio, Ciudad, Provincia, Dirección y estado de actividad.
2. **Categoria**: Rubros de productos (Herramientas, Alimentos Pampeanos, Indumentaria de Trabajo, Repuestos, etc.).
3. **Provincia**: Provincias integradas en la red del centro del país (La Pampa, Córdoba, Buenos Aires, San Luis).
4. **NodoDistribucion**: Centros logísticos asociados a una provincia con tarifa base, tiempo de entrega y geolocalización.
5. **Producto**: Publicaciones con precio, stock, código SKU, peso en kg, visitas, estado activo y relación con categoría y comercio vendedor.
6. **Pedido**: Transacción logística con código de seguimiento, comprador, vendedor, nodo asignado, subtotales, costo de flete, token criptográfico y QR en Base64.
7. **DetallePedido**: Items individuales de cada orden con cantidad, precio histórico y subtotal.
8. **HistorialSeguimiento**: Bitácora de eventos cronológicos del envío con operador responsable, ubicación y descripción.
9. **ConsultaSoporte**: Mensajería interna entre comprador, comercio o administración con soporte para indicador de urgencia.

---

## 4. Guía de Instalación y Ejecución Local

### Requisitos Previos

- .NET 10 SDK instalado en el equipo.
- SQL Server LocalDB (incluido con Visual Studio o SQL Server Management Studio) o una instancia de SQL Server Express.
- Git instalado (opcional, para clonar el repositorio).

### Paso 1: Clonar el Repositorio

Abrir una consola de comandos (PowerShell, CMD o Terminal de Linux/macOS) y ejecutar:

```bash
git clone https://github.com/tu-usuario/LCP.git
cd LCP
```

### Paso 2: Configuración de la Cadena de Conexión

El archivo `LCP.Web/appsettings.json` incluye por defecto la cadena de conexión a SQL Server LocalDB:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LCP_LogisticaCentroPaisDb;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=\"SQL Server Management Studio\";Command Timeout=0"
  }
}
```

Si se utiliza una instancia diferente de SQL Server, ajustar el valor de `DefaultConnection` en `appsettings.json` o en `appsettings.Development.json`.

### Paso 3: Restaurar Paquetes y Compilar

Navegar a la carpeta del proyecto web y compilar la solución:

```bash
cd LCP.Web
dotnet restore
dotnet build
```

### Paso 4: Aplicar Migraciones y Cargar Datos Iniciales

La base de datos y los datos de prueba se inicializan automáticamente al ejecutar la aplicación mediante `DbInitializer.cs`.

De manera alternativa, se pueden aplicar las 4 migraciones manualmente mediante la CLI de Entity Framework Core:

```bash
dotnet ef database update
```

### Paso 5: Iniciar la Aplicación

Ejecutar el servidor local de desarrollo Kestrel:

```bash
dotnet run
```

Abrir un navegador web y acceder a la dirección local informada en consola:

- HTTP: `http://localhost:5235`
- HTTPS: `https://localhost:7065`

---

## 5. Credenciales de Prueba Pre-cargadas

El Data Seeding inicializa los siguientes usuarios para evaluar todos los roles del sistema:

| Rol | Correo Electrónico | Contraseña | Nombre / Entidad |
|---|---|---|---|
| **Administrador** | `admin@lcp.com.ar` | `Admin123!` | Juan Vázquez (Administración Central LCP) |
| **Vendedor** | `vendedor1@lcp.com.ar` | `Vendedor123!` | Mariano López (Distribuidora Pampeana Central - Santa Rosa) |
| **Vendedor** | `vendedor2@lcp.com.ar` | `Vendedor123!` | Roberto Rossi (AgroLogística Centro - General Pico) |
| **Comprador** | `comprador@lcp.com.ar` | `Comprador123!` | Carlos Gómez (Cliente particular) |

---

## 6. Procedimientos Almacenados (Stored Procedures)

Los Stored Procedures se crean automáticamente en SQL Server durante el arranque de la aplicación a través de `IEstadisticasService`:

1. **`sp_ObtenerEstadisticasVendedor`**:
   - **Objetivo**: Genera métricas agregadas de ventas netas, conteo de pedidos discriminados por estado, stock de productos activos y rubro más vendido.
   - **Parámetro**: `@VendedorId NVARCHAR(450) = NULL` (si no se especifica, calcula las métricas globales del sistema).
   - **Consumo en C#**: `_context.Database.SqlQueryRaw<EstadisticaVentaDto>("EXEC sp_ObtenerEstadisticasVendedor @VendedorId", param)`.

2. **`sp_ListarPedidosPorEstado`**:
   - **Objetivo**: Lista órdenes con detalle del comprador, comercio vendedor, provincia, nodo de entrega y conteo de items.
   - **Parámetros**: `@EstadoId INT = NULL`, `@VendedorId NVARCHAR(450) = NULL`.
   - **Consumo en C#**: `_context.Database.SqlQueryRaw<PedidoReporteSpDto>("EXEC sp_ListarPedidosPorEstado @EstadoId, @VendedorId", pEstado, pVendedor)`.

---

## 7. Endpoints de la API REST

La plataforma expone controladores de API RESTful con respuestas en formato JSON:

- **`GET /api/v1/productos`**: Retorna el catálogo de publicaciones activas.
  - Parámetros opcionales de consulta: `?categoriaId=1&q=miel`
- **`GET /api/v1/productos/{id}`**: Retorna los datos detallados de un producto específico.
- **`GET /api/v1/tracking/{codigo}`**: Retorna el estado en tiempo real, origen, destino, nodo asignado e historial cronológico de un paquete a partir de su código de seguimiento.

---

## 8. Estructura del Proyecto

```text
LCP/
├── LCP.sln
├── .gitignore
├── README.md
└── LCP.Web/
    ├── Controllers/             # Controladores MVC y API (Admin, Vendedor, Catalogo, etc.)
    ├── Data/                    # DbContext y DbInitializer (Seed Data)
    ├── Migrations/              # 4 Migraciones secuenciales de EF Core
    ├── Models/
    │   ├── Entities/            # Entidades de dominio con DataAnnotations
    │   ├── Enums/               # Enumeraciones de estado, pago y roles
    │   └── ViewModels/          # Modelos fuertemente tipados por módulo
    ├── Services/                # Servicios DI (Pedidos, Estadísticas SP, QR, Archivos)
    ├── ViewComponents/          # Componentes Razor (CarritoBadge, TrackingTimeline)
    ├── Views/                   # Vistas Razor estructuradas por controlador
    │   └── Shared/              # _Layout responsivo, Navbar y parciales
    ├── wwwroot/                 # Archivos estáticos (CSS, JS, imágenes corporativas, uploads)
    ├── appsettings.json         # Configuración y cadena de conexión a base de datos
    └── Program.cs               # Pipeline HTTP, inyección de dependencias y middlewares
```

---

## 9. Información Académica

- **Institución**: ITES
- **Materia**: Programación III - Aplicaciones Web
- **Docente**: Federico Trani
- **Estudiante**: Juan Vázquez
- **Proyecto**: Logística Centro País (LCP)
- **Año lectivo**: 2026
