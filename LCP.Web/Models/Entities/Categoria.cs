using System.ComponentModel.DataAnnotations;

namespace LCP.Web.Models.Entities
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [Display(Name = "Nombre de la Categoría")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [StringLength(100, ErrorMessage = "El icono no puede superar los 100 caracteres")]
        [Display(Name = "Icono CSS / FontAwesome")]
        public string? Icono { get; set; } = "fas fa-box";

        [Display(Name = "¿Activa?")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relación 1 a N con Productos
        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
