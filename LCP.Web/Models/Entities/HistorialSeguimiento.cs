using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LCP.Web.Models.Enums;

namespace LCP.Web.Models.Entities
{
    public class HistorialSeguimiento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public virtual Pedido? Pedido { get; set; }

        [Display(Name = "Fecha y Hora")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(150)]
        [Display(Name = "Ubicación / Nodo")]
        public string Ubicacion { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        [Display(Name = "Detalle del Evento")]
        public string Descripcion { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Operador / Agente Logístico")]
        public string? OperadorResponsable { get; set; } = "Sistema Central LCP";

        [Display(Name = "Estado")]
        public EstadoPedido Estado { get; set; }
    }
}
