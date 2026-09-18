using LCP.Web.Data;
using LCP.Web.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Services
{
    public interface IEstadisticasService
    {
        Task<EstadisticaVentaDto> ObtenerEstadisticasSpAsync(string? vendedorId = null);
        Task<List<PedidoReporteSpDto>> ListarPedidosReporteSpAsync(int? estadoId = null, string? vendedorId = null);
        Task CrearStoredProceduresSiNoExistenAsync();
    }

    public class EstadisticasService : IEstadisticasService
    {
        private readonly ApplicationDbContext _context;

        public EstadisticasService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CrearStoredProceduresSiNoExistenAsync()
        {
            var spEstadisticasSql = @"
CREATE OR ALTER PROCEDURE sp_ObtenerEstadisticasVendedor
    @VendedorId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalPedidos INT = 0;
    DECLARE @TotalVentas DECIMAL(18,2) = 0;
    DECLARE @PedidosEnProceso INT = 0;
    DECLARE @PedidosEnPreparacion INT = 0;
    DECLARE @PedidosEnCamino INT = 0;
    DECLARE @PedidosEntregados INT = 0;
    DECLARE @PedidosCancelados INT = 0;
    DECLARE @TotalProductos INT = 0;
    DECLARE @CategoriaMasVendida NVARCHAR(150) = 'Sin ventas registradas';

    -- Conteos y sumas de pedidos
    SELECT 
        @TotalPedidos = COUNT(p.Id),
        @TotalVentas = ISNULL(SUM(CASE WHEN p.Estado != 5 THEN p.Total ELSE 0 END), 0),
        @PedidosEnProceso = COUNT(CASE WHEN p.Estado = 1 THEN 1 END),
        @PedidosEnPreparacion = COUNT(CASE WHEN p.Estado = 2 THEN 1 END),
        @PedidosEnCamino = COUNT(CASE WHEN p.Estado = 3 THEN 1 END),
        @PedidosEntregados = COUNT(CASE WHEN p.Estado = 4 THEN 1 END),
        @PedidosCancelados = COUNT(CASE WHEN p.Estado = 5 THEN 1 END)
    FROM Pedidos p
    WHERE (@VendedorId IS NULL OR p.VendedorId = @VendedorId);

    -- Total productos activos
    SELECT @TotalProductos = COUNT(pr.Id)
    FROM Productos pr
    WHERE (@VendedorId IS NULL OR pr.VendedorId = @VendedorId) AND pr.Activo = 1;

    -- Categoría más vendida
    SELECT TOP 1 @CategoriaMasVendida = c.Nombre
    FROM DetallesPedido dp
    INNER JOIN Pedidos p ON dp.PedidoId = p.Id
    INNER JOIN Productos pr ON dp.ProductoId = pr.Id
    INNER JOIN Categorias c ON pr.CategoriaId = c.Id
    WHERE (@VendedorId IS NULL OR p.VendedorId = @VendedorId) AND p.Estado != 5
    GROUP BY c.Nombre
    ORDER BY SUM(dp.Cantidad) DESC;

    SELECT 
        @TotalPedidos AS TotalPedidos,
        @TotalVentas AS TotalVentas,
        @PedidosEnProceso AS PedidosEnProceso,
        @PedidosEnPreparacion AS PedidosEnPreparacion,
        @PedidosEnCamino AS PedidosEnCamino,
        @PedidosEntregados AS PedidosEntregados,
        @PedidosCancelados AS PedidosCancelados,
        @TotalProductos AS TotalProductos,
        ISNULL(@CategoriaMasVendida, 'N/A') AS CategoriaMasVendida;
END";

            var spListadoPedidosSql = @"
CREATE OR ALTER PROCEDURE sp_ListarPedidosPorEstado
    @EstadoId INT = NULL,
    @VendedorId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.Id AS PedidoId,
        p.CodigoSeguimiento,
        p.FechaPedido,
        ISNULL(uComp.Nombre + ' ' + uComp.Apellido, 'Cliente') AS NombreComprador,
        ISNULL(uVend.NombreComercio, uVend.Nombre + ' ' + uVend.Apellido) AS NombreVendedor,
        p.CiudadEntrega,
        p.ProvinciaEntrega,
        CAST(p.Estado AS INT) AS EstadoId,
        CASE p.Estado
            WHEN 1 THEN 'En proceso'
            WHEN 2 THEN 'En preparación'
            WHEN 3 THEN 'En camino'
            WHEN 4 THEN 'Entregado'
            WHEN 5 THEN 'Cancelado'
            ELSE 'Desconocido'
        END AS EstadoNombre,
        p.Total,
        ISNULL((SELECT SUM(dp.Cantidad) FROM DetallesPedido dp WHERE dp.PedidoId = p.Id), 0) AS CantidadItems
    FROM Pedidos p
    INNER JOIN AspNetUsers uComp ON p.CompradorId = uComp.Id
    INNER JOIN AspNetUsers uVend ON p.VendedorId = uVend.Id
    WHERE (@EstadoId IS NULL OR CAST(p.Estado AS INT) = @EstadoId)
      AND (@VendedorId IS NULL OR p.VendedorId = @VendedorId)
    ORDER BY p.FechaPedido DESC;
END";

            try
            {
                await _context.Database.ExecuteSqlRawAsync(spEstadisticasSql);
                await _context.Database.ExecuteSqlRawAsync(spListadoPedidosSql);
            }
            catch
            {
                // Manejado si no se encuentra conexión activa en tiempo de diseño
            }
        }

        public async Task<EstadisticaVentaDto> ObtenerEstadisticasSpAsync(string? vendedorId = null)
        {
            var param = new SqlParameter("@VendedorId", (object?)vendedorId ?? DBNull.Value);
            var resultados = await _context.Database
                .SqlQueryRaw<EstadisticaVentaDto>("EXEC sp_ObtenerEstadisticasVendedor @VendedorId", param)
                .ToListAsync();

            return resultados.FirstOrDefault() ?? new EstadisticaVentaDto();
        }

        public async Task<List<PedidoReporteSpDto>> ListarPedidosReporteSpAsync(int? estadoId = null, string? vendedorId = null)
        {
            var pEstado = new SqlParameter("@EstadoId", (object?)estadoId ?? DBNull.Value);
            var pVendedor = new SqlParameter("@VendedorId", (object?)vendedorId ?? DBNull.Value);

            var resultados = await _context.Database
                .SqlQueryRaw<PedidoReporteSpDto>("EXEC sp_ListarPedidosPorEstado @EstadoId, @VendedorId", pEstado, pVendedor)
                .ToListAsync();

            return resultados;
        }
    }
}
