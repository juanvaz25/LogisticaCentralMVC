using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LCP.Web.Models.Entities
{
    public class NodoDistribucion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del nodo es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
        [Display(Name = "Nombre del Centro / Nodo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección física es obligatoria")]
        [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad o localidad es obligatoria")]
        [StringLength(100, ErrorMessage = "La ciudad no puede superar los 100 caracteres")]
        [Display(Name = "Ciudad / Localidad")]
        public string Ciudad { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El código postal no puede superar los 20 caracteres")]
        [Display(Name = "Código Postal")]
        public string? CodigoPostal { get; set; }

        [StringLength(50, ErrorMessage = "El teléfono no puede superar los 50 caracteres")]
        [Display(Name = "Teléfono de Contacto")]
        public string? Telefono { get; set; }

        [StringLength(100, ErrorMessage = "El horario no puede superar los 100 caracteres")]
        [Display(Name = "Horario de Atención")]
        public string? HorarioAtencion { get; set; } = "Lunes a Viernes 08:00 a 18:00 hs";

        [Range(1, 240, ErrorMessage = "El tiempo estimado de entrega debe estar entre 1 y 240 horas")]
        [Display(Name = "Tiempo Estimado de Entrega (Horas)")]
        public int TiempoEstimadoHoras { get; set; } = 24;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000000, ErrorMessage = "La tarifa base debe ser positiva")]
        [Display(Name = "Costo de Envío Base ($)")]
        public decimal TarifaBase { get; set; } = 2500m;

        [Display(Name = "¿Activo?")]
        public bool Activo { get; set; } = true;

        [Required(ErrorMessage = "La provincia es obligatoria")]
        [Display(Name = "Provincia")]
        public int ProvinciaId { get; set; }

        // Navegación
        [ForeignKey("ProvinciaId")]
        public virtual Provincia? Provincia { get; set; }

        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
