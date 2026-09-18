using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LCP.Web.Models.Entities
{
    public class ConsultaSoporte
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El asunto de la consulta es obligatorio")]
        [StringLength(150, ErrorMessage = "El asunto no puede superar los 150 caracteres")]
        [Display(Name = "Asunto")]
        public string Asunto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mensaje es obligatorio")]
        [StringLength(2000, ErrorMessage = "El mensaje no puede superar los 2000 caracteres")]
        [Display(Name = "Mensaje o Consulta")]
        public string Mensaje { get; set; } = string.Empty;

        [Display(Name = "Fecha de Envío")]
        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;

        [Display(Name = "¿Respondida?")]
        public bool Respondido { get; set; } = false;

        [Display(Name = "¿Consulta Urgente?")]
        public bool EsUrgente { get; set; } = false;

        [StringLength(2000)]
        [Display(Name = "Respuesta")]
        public string? Respuesta { get; set; }

        [Display(Name = "Fecha de Respuesta")]
        public DateTime? FechaRespuesta { get; set; }

        // Emisor (Comprador o Vendedor)
        [Required]
        public string EmisorId { get; set; } = string.Empty;

        [ForeignKey("EmisorId")]
        public virtual ApplicationUser? Emisor { get; set; }

        // Receptor opcional (Vendedor destinatario o null si es soporte general LCP)
        public string? ReceptorId { get; set; }

        [ForeignKey("ReceptorId")]
        public virtual ApplicationUser? Receptor { get; set; }

        // Producto asociado opcional
        public int? ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto? Producto { get; set; }
    }
}
