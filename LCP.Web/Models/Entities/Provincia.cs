using System.ComponentModel.DataAnnotations;

namespace LCP.Web.Models.Entities
{
    public class Provincia
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la provincia es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        [Display(Name = "Provincia")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "El código no puede superar los 10 caracteres")]
        [Display(Name = "Código")]
        public string? Codigo { get; set; }

        public bool Activo { get; set; } = true;

        // Relación 1 a N con Nodos de Distribución
        public virtual ICollection<NodoDistribucion> Nodos { get; set; } = new List<NodoDistribucion>();
    }
}
