using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LCP.Web.Models.Enums;

namespace LCP.Web.Models.Entities
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Código de Seguimiento")]
        public string CodigoSeguimiento { get; set; } = string.Empty;

        [Display(Name = "Fecha del Pedido")]
        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

        [Display(Name = "Entrega Estimada")]
        public DateTime? FechaEntregaEstimada { get; set; }

        [Display(Name = "Fecha de Entrega Real")]
        public DateTime? FechaEntregaReal { get; set; }

        [Required]
        [Display(Name = "Estado del Envío / Pedido")]
        public EstadoPedido Estado { get; set; } = EstadoPedido.EnProceso;

        [StringLength(500, ErrorMessage = "La observación de cancelación no puede superar los 500 caracteres")]
        [Display(Name = "Motivo u Observación de Cancelación")]
        public string? MotivoCancelacion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal ($)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Costo de Envío Logístico ($)")]
        public decimal CostoEnvio { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Final ($)")]
        public decimal Total { get; set; }

        [Range(1, 5, ErrorMessage = "La calificación debe ser de 1 a 5 estrellas")]
        [Display(Name = "Calificación del Servicio")]
        public int? CalificacionComprador { get; set; }

        [Display(Name = "Última Actualización")]
        public DateTime? FechaUltimaActualizacion { get; set; } = DateTime.UtcNow;

        [Display(Name = "Método de Pago")]
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Transferencia;

        [Required(ErrorMessage = "La dirección de entrega es obligatoria")]
        [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres")]
        [Display(Name = "Dirección de Entrega")]
        public string DireccionEntrega { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        [StringLength(100, ErrorMessage = "La ciudad no puede superar los 100 caracteres")]
        [Display(Name = "Ciudad")]
        public string CiudadEntrega { get; set; } = string.Empty;

        [Required(ErrorMessage = "La provincia es obligatoria")]
        [StringLength(100, ErrorMessage = "La provincia no puede superar los 100 caracteres")]
        [Display(Name = "Provincia")]
        public string ProvinciaEntrega { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Código Postal")]
        public string? CodigoPostalEntrega { get; set; }

        [Required(ErrorMessage = "El teléfono de contacto es obligatorio")]
        [StringLength(50, ErrorMessage = "El teléfono no puede superar los 50 caracteres")]
        [Display(Name = "Teléfono de Contacto")]
        public string TelefonoContacto { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Notas / Instrucciones de Entrega")]
        public string? NotasAdicionales { get; set; }

        [Display(Name = "Código QR de Confirmación de Entrega")]
        public string? CodigoQrBase64 { get; set; }

        [Display(Name = "Hash / Token de Entrega QR")]
        public string? TokenEntregaQr { get; set; }

        // Nodo Logístico
        [Display(Name = "Nodo de Distribución LCP")]
        public int? NodoDistribucionId { get; set; }

        [ForeignKey("NodoDistribucionId")]
        public virtual NodoDistribucion? NodoDistribucion { get; set; }

        // Comprador y Vendedor
        [Required]
        [Display(Name = "Comprador")]
        public string CompradorId { get; set; } = string.Empty;

        [ForeignKey("CompradorId")]
        public virtual ApplicationUser? Comprador { get; set; }

        [Required]
        [Display(Name = "Vendedor")]
        public string VendedorId { get; set; } = string.Empty;

        [ForeignKey("VendedorId")]
        public virtual ApplicationUser? Vendedor { get; set; }

        // Relaciones 1 a N
        public virtual ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
        public virtual ICollection<HistorialSeguimiento> Historial { get; set; } = new List<HistorialSeguimiento>();
    }
}
