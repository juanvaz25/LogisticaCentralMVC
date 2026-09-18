using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace LCP.Web.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El DNI/Documento no puede superar los 20 caracteres")]
        [Display(Name = "DNI / Documento")]
        public string? Dni { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [StringLength(100, ErrorMessage = "La ciudad no puede superar los 100 caracteres")]
        [Display(Name = "Ciudad")]
        public string? Ciudad { get; set; }

        [StringLength(100, ErrorMessage = "La provincia no puede superar los 100 caracteres")]
        [Display(Name = "Provincia")]
        public string? Provincia { get; set; }

        [StringLength(150, ErrorMessage = "El nombre del comercio no puede superar los 150 caracteres")]
        [Display(Name = "Nombre de Fantasía / Comercio")]
        public string? NombreComercio { get; set; }

        [StringLength(30, ErrorMessage = "El CUIT/CUIL no puede superar los 30 caracteres")]
        [Display(Name = "CUIT / CUIL")]
        public string? Cuit { get; set; }

        [Display(Name = "Foto de Perfil")]
        public string? FotoPerfilUrl { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Display(Name = "¿Vendedor Habilitado?")]
        public bool EsVendedorActivo { get; set; } = false;

        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

        // Relaciones de navegación
        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public virtual ICollection<Pedido> PedidosComprados { get; set; } = new List<Pedido>();
        public virtual ICollection<Pedido> PedidosVendidos { get; set; } = new List<Pedido>();
        public virtual ICollection<ConsultaSoporte> ConsultasEnviadas { get; set; } = new List<ConsultaSoporte>();
        public virtual ICollection<ConsultaSoporte> ConsultasRecibidas { get; set; } = new List<ConsultaSoporte>();
    }
}
