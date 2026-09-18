using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LCP.Web.Models.ViewModels.Nodos
{
    public class NodoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del nodo es obligatorio")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
        [Display(Name = "Nombre del Centro / Nodo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección física es obligatoria")]
        [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad o localidad es obligatoria")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Ciudad / Localidad")]
        public string Ciudad { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Código Postal")]
        public string? CodigoPostal { get; set; }

        [StringLength(50)]
        [Display(Name = "Teléfono de Contacto")]
        public string? Telefono { get; set; }

        [Range(1, 240, ErrorMessage = "Entre 1 y 240 horas")]
        [Display(Name = "Tiempo Estimado de Entrega (Horas)")]
        public int TiempoEstimadoHoras { get; set; } = 24;

        [Range(0, 1000000, ErrorMessage = "Debe ser mayor o igual a 0")]
        [Display(Name = "Tarifa Base de Envío ($)")]
        public decimal TarifaBase { get; set; } = 2500m;

        [Display(Name = "¿Nodo Activo?")]
        public bool Activo { get; set; } = true;

        [Required(ErrorMessage = "La provincia es obligatoria")]
        [Display(Name = "Provincia")]
        public int ProvinciaId { get; set; }

        public string? NombreProvincia { get; set; }

        public IEnumerable<SelectListItem>? ProvinciasList { get; set; }
    }
}
