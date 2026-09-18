using System.ComponentModel.DataAnnotations;
using LCP.Web.Models.Entities;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace LCP.Web.Models.ViewModels.Consultas
{
    public class ConsultaCreateViewModel
    {
        public int? ProductoId { get; set; }
        public string? TituloProducto { get; set; }
        
        [Display(Name = "Destinatario de la consulta")]
        public string? ReceptorId { get; set; }
        public string? NombreDestinatario { get; set; }
        public List<SelectListItem> DestinatariosList { get; set; } = new();

        [Required(ErrorMessage = "El asunto es obligatorio")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
        [Display(Name = "Asunto de la Consulta")]
        public string Asunto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mensaje no puede estar vacío")]
        [StringLength(2000, ErrorMessage = "Máximo 2000 caracteres")]
        [Display(Name = "Mensaje o Pregunta")]
        public string Mensaje { get; set; } = string.Empty;

        [Display(Name = "¿Es una consulta urgente de logística/entrega?")]
        public bool EsUrgente { get; set; } = false;
    }

    public class ConsultaResponderViewModel
    {
        public int ConsultaId { get; set; }
        public ConsultaSoporte? Consulta { get; set; }

        [Required(ErrorMessage = "La respuesta es obligatoria")]
        [StringLength(2000, ErrorMessage = "Máximo 2000 caracteres")]
        [Display(Name = "Escribir Respuesta")]
        public string Respuesta { get; set; } = string.Empty;
    }
}
