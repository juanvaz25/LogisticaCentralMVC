using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LCP.Web.Models.Entities
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la publicación es obligatorio")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres")]
        [Display(Name = "Título del Producto / Publicación")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(2000, ErrorMessage = "La descripción no puede superar los 2000 caracteres")]
        [Display(Name = "Descripción Detallada")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 100000000, ErrorMessage = "El precio debe ser mayor a 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio Unitario ($)")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El stock disponible es obligatorio")]
        [Range(0, 100000, ErrorMessage = "El stock debe ser un valor no negativo")]
        [Display(Name = "Stock Disponible")]
        public int Stock { get; set; } = 1;

        [StringLength(500, ErrorMessage = "La URL de la imagen no puede superar los 500 caracteres")]
        [Display(Name = "Foto / Imagen del Producto")]
        public string? ImagenUrl { get; set; }

        [StringLength(50, ErrorMessage = "El código SKU no puede superar los 50 caracteres")]
        [Display(Name = "Código SKU / Referencia")]
        public string? CodigoSku { get; set; }

        [Range(0.01, 1000, ErrorMessage = "El peso estimado debe ser mayor a 0")]
        [Column(TypeName = "decimal(8,2)")]
        [Display(Name = "Peso Estimado (kg)")]
        public decimal PesoKg { get; set; } = 1.0m;

        [Display(Name = "Fecha de Publicación")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Display(Name = "¿Publicación Activa?")]
        public bool Activo { get; set; } = true;

        [Display(Name = "¿Producto Destacado?")]
        public bool Destacado { get; set; } = false;

        [Display(Name = "Cantidad de Visitas")]
        public int NumeroVisitas { get; set; } = 0;

        // Claves Foráneas y Relaciones
        [Required(ErrorMessage = "Debe seleccionar una categoría")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }

        [Required(ErrorMessage = "El vendedor es obligatorio")]
        [Display(Name = "Vendedor")]
        public string VendedorId { get; set; } = string.Empty;

        [ForeignKey("VendedorId")]
        public virtual ApplicationUser? Vendedor { get; set; }

        // Relaciones de navegación
        public virtual ICollection<DetallePedido> DetallesPedido { get; set; } = new List<DetallePedido>();
        public virtual ICollection<ConsultaSoporte> Consultas { get; set; } = new List<ConsultaSoporte>();
    }
}
