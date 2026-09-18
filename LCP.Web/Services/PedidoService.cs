using LCP.Web.Data;
using LCP.Web.Models.Entities;
using LCP.Web.Models.Enums;
using LCP.Web.Models.ViewModels.Pedidos;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Services
{
    public interface IPedidoService
    {
        Task<Pedido> CrearPedidoDesdeCheckoutAsync(CheckoutViewModel model, string compradorId);
        Task<bool> CambiarEstadoAsync(int pedidoId, EstadoPedido nuevoEstado, string? motivoCancelacion, string? ubicacion, string usuarioModificadorId);
        Task<bool> ConfirmarEntregaPorQrAsync(string tokenEntrega, string usuarioValidadorId);
        Task<Pedido?> ObtenerPorCodigoTrackingAsync(string codigoTracking);
        string GenerarCodigoSeguimiento();
    }

    public class PedidoService : IPedidoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IQRService _qrService;

        public PedidoService(ApplicationDbContext context, IQRService qrService)
        {
            _context = context;
            _qrService = qrService;
        }

        public string GenerarCodigoSeguimiento()
        {
            var fecha = DateTime.UtcNow.ToString("yyyyMMdd");
            var aleatorio = new Random().Next(1000, 9999);
            return $"LCP-{fecha}-{aleatorio}";
        }

        public async Task<Pedido> CrearPedidoDesdeCheckoutAsync(CheckoutViewModel model, string compradorId)
        {
            if (model.Items == null || !model.Items.Any())
                throw new InvalidOperationException("El carrito de compras está vacío.");

            // Tomar el vendedor del primer producto (o del grupo)
            var vendedorId = model.Items.First().VendedorId;

            // Obtener datos del nodo para estimar tiempos
            var nodo = await _context.NodosDistribucion
                                     .Include(n => n.Provincia)
                                     .FirstOrDefaultAsync(n => n.Id == model.NodoDistribucionId);

            var horasEstimadas = nodo?.TiempoEstimadoHoras ?? 24;
            var costoEnvio = nodo?.TarifaBase ?? 2500m;

            var subtotal = model.Items.Sum(i => i.Subtotal);
            var total = subtotal + costoEnvio;

            var trackingCode = GenerarCodigoSeguimiento();
            var tokenEntrega = Guid.NewGuid().ToString("N")[..12].ToUpper();

            // Generar URL o texto del código QR para validación de entrega
            var qrContent = $"LCP-ENTREGA:{trackingCode}:{tokenEntrega}";
            var qrBase64 = _qrService.GenerarQrBase64(qrContent);

            var pedido = new Pedido
            {
                CodigoSeguimiento = trackingCode,
                FechaPedido = DateTime.UtcNow,
                FechaEntregaEstimada = DateTime.UtcNow.AddHours(horasEstimadas),
                Estado = EstadoPedido.EnProceso,
                Subtotal = subtotal,
                CostoEnvio = costoEnvio,
                Total = total,
                MetodoPago = model.MetodoPago,
                DireccionEntrega = model.DireccionEntrega,
                CiudadEntrega = model.CiudadEntrega,
                ProvinciaEntrega = nodo?.Provincia?.Nombre ?? "La Pampa",
                CodigoPostalEntrega = model.CodigoPostal,
                TelefonoContacto = model.TelefonoContacto,
                NotasAdicionales = model.NotasAdicionales,
                CodigoQrBase64 = qrBase64,
                TokenEntregaQr = tokenEntrega,
                NodoDistribucionId = model.NodoDistribucionId,
                CompradorId = compradorId,
                VendedorId = vendedorId
            };

            foreach (var item in model.Items)
            {
                pedido.Detalles.Add(new DetallePedido
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.Precio,
                    Subtotal = item.Subtotal
                });

                // Descontar stock
                var prod = await _context.Productos.FindAsync(item.ProductoId);
                if (prod != null)
                {
                    prod.Stock = Math.Max(0, prod.Stock - item.Cantidad);
                }
            }

            // Primer hito de seguimiento
            pedido.Historial.Add(new HistorialSeguimiento
            {
                Fecha = DateTime.UtcNow,
                Ubicacion = nodo?.Nombre ?? "Centro Logístico LCP",
                Descripcion = "Pedido recibido y confirmado en el sistema LCP",
                Estado = EstadoPedido.EnProceso
            });

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return pedido;
        }

        public async Task<bool> CambiarEstadoAsync(int pedidoId, EstadoPedido nuevoEstado, string? motivoCancelacion, string? ubicacion, string usuarioModificadorId)
        {
            var pedido = await _context.Pedidos
                                       .Include(p => p.NodoDistribucion)
                                       .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null)
                return false;

            if (nuevoEstado == EstadoPedido.Cancelado && string.IsNullOrWhiteSpace(motivoCancelacion))
                throw new InvalidOperationException("Debe ingresar un motivo u observación para cancelar el pedido.");

            pedido.Estado = nuevoEstado;
            if (nuevoEstado == EstadoPedido.Cancelado)
            {
                pedido.MotivoCancelacion = motivoCancelacion;
            }
            else if (nuevoEstado == EstadoPedido.Entregado)
            {
                pedido.FechaEntregaReal = DateTime.UtcNow;
            }

            string detalleEvento = nuevoEstado switch
            {
                EstadoPedido.EnProceso => "El pedido fue registrado y está en proceso de gestión.",
                EstadoPedido.EnPreparacion => "El vendedor aceptó el pedido y se encuentra en empaque y preparación.",
                EstadoPedido.EnCamino => "El paquete fue despachado y está en viaje hacia el destino.",
                EstadoPedido.Entregado => "El paquete fue entregado con éxito al comprador.",
                EstadoPedido.Cancelado => $"El pedido fue cancelado. Motivo: {motivoCancelacion}",
                _ => "Actualización de estado del envío"
            };

            var ub = !string.IsNullOrWhiteSpace(ubicacion) ? ubicacion : (pedido.NodoDistribucion?.Nombre ?? "Nodo LCP");

            pedido.Historial.Add(new HistorialSeguimiento
            {
                PedidoId = pedido.Id,
                Fecha = DateTime.UtcNow,
                Ubicacion = ub,
                Descripcion = detalleEvento,
                Estado = nuevoEstado
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmarEntregaPorQrAsync(string tokenEntrega, string usuarioValidadorId)
        {
            if (string.IsNullOrWhiteSpace(tokenEntrega))
                return false;

            var pedido = await _context.Pedidos
                                       .Include(p => p.NodoDistribucion)
                                       .FirstOrDefaultAsync(p => p.TokenEntregaQr == tokenEntrega.Trim());

            if (pedido == null)
                return false;

            if (pedido.Estado == EstadoPedido.Entregado)
                return true; // Ya estaba entregado

            return await CambiarEstadoAsync(pedido.Id, EstadoPedido.Entregado, null, "Punto de Entrega al Comprador (Validación QR)", usuarioValidadorId);
        }

        public async Task<Pedido?> ObtenerPorCodigoTrackingAsync(string codigoTracking)
        {
            if (string.IsNullOrWhiteSpace(codigoTracking))
                return null;

            return await _context.Pedidos
                                 .Include(p => p.Comprador)
                                 .Include(p => p.Vendedor)
                                 .Include(p => p.NodoDistribucion)
                                 .Include(p => p.Detalles)
                                    .ThenInclude(d => d.Producto)
                                 .Include(p => p.Historial.OrderByDescending(h => h.Fecha))
                                 .FirstOrDefaultAsync(p => p.CodigoSeguimiento.ToLower() == codigoTracking.Trim().ToLower());
        }
    }
}
