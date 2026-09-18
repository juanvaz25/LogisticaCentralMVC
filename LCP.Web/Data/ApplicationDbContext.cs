using LCP.Web.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LCP.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Provincia> Provincias { get; set; } = null!;
        public DbSet<NodoDistribucion> NodosDistribucion { get; set; } = null!;
        public DbSet<Producto> Productos { get; set; } = null!;
        public DbSet<Pedido> Pedidos { get; set; } = null!;
        public DbSet<DetallePedido> DetallesPedido { get; set; } = null!;
        public DbSet<HistorialSeguimiento> HistorialSeguimientos { get; set; } = null!;
        public DbSet<ConsultaSoporte> ConsultasSoporte { get; set; } = null!;

        // DTOs para Stored Procedures (sin clave primaria)
        public DbSet<EstadisticaVentaDto> EstadisticasVentasSp { get; set; } = null!;
        public DbSet<PedidoReporteSpDto> PedidosReporteSp { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Mapeo DTOs Stored Procedures
            builder.Entity<EstadisticaVentaDto>(e => {
                e.HasNoKey();
                e.Property(p => p.TotalVentas).HasPrecision(18, 2);
            });
            builder.Entity<PedidoReporteSpDto>(e => {
                e.HasNoKey();
                e.Property(p => p.Total).HasPrecision(18, 2);
            });

            // Configuración de Producto
            builder.Entity<Producto>(entity =>
            {
                entity.Property(p => p.Precio).HasPrecision(18, 2);
                entity.Property(p => p.PesoKg).HasPrecision(8, 2);

                entity.HasOne(p => p.Categoria)
                      .WithMany(c => c.Productos)
                      .HasForeignKey(p => p.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Vendedor)
                      .WithMany(u => u.Productos)
                      .HasForeignKey(p => p.VendedorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Pedido
            builder.Entity<Pedido>(entity =>
            {
                entity.Property(p => p.Subtotal).HasPrecision(18, 2);
                entity.Property(p => p.CostoEnvio).HasPrecision(18, 2);
                entity.Property(p => p.Total).HasPrecision(18, 2);

                entity.HasOne(p => p.Comprador)
                      .WithMany(u => u.PedidosComprados)
                      .HasForeignKey(p => p.CompradorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Vendedor)
                      .WithMany(u => u.PedidosVendidos)
                      .HasForeignKey(p => p.VendedorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.NodoDistribucion)
                      .WithMany(n => n.Pedidos)
                      .HasForeignKey(p => p.NodoDistribucionId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(p => p.CodigoSeguimiento).IsUnique();
            });

            // Configuración de DetallePedido
            builder.Entity<DetallePedido>(entity =>
            {
                entity.Property(d => d.PrecioUnitario).HasPrecision(18, 2);
                entity.Property(d => d.Subtotal).HasPrecision(18, 2);

                entity.HasOne(d => d.Pedido)
                      .WithMany(p => p.Detalles)
                      .HasForeignKey(d => d.PedidoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Producto)
                      .WithMany(p => p.DetallesPedido)
                      .HasForeignKey(d => d.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de HistorialSeguimiento
            builder.Entity<HistorialSeguimiento>(entity =>
            {
                entity.HasOne(h => h.Pedido)
                      .WithMany(p => p.Historial)
                      .HasForeignKey(h => h.PedidoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de ConsultaSoporte
            builder.Entity<ConsultaSoporte>(entity =>
            {
                entity.HasOne(c => c.Emisor)
                      .WithMany(u => u.ConsultasEnviadas)
                      .HasForeignKey(c => c.EmisorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Receptor)
                      .WithMany(u => u.ConsultasRecibidas)
                      .HasForeignKey(c => c.ReceptorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Producto)
                      .WithMany(p => p.Consultas)
                      .HasForeignKey(c => c.ProductoId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de Nodos
            builder.Entity<NodoDistribucion>(entity =>
            {
                entity.Property(n => n.TarifaBase).HasPrecision(18, 2);

                entity.HasOne(n => n.Provincia)
                      .WithMany(p => p.Nodos)
                      .HasForeignKey(n => n.ProvinciaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
