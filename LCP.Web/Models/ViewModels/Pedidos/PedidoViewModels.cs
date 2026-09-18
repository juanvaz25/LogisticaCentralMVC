using System.ComponentModel.DataAnnotations;
using LCP.Web.Models.Entities;
using LCP.Web.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LCP.Web.Models.ViewModels.Pedidos
{
    public class CarritoItemViewModel
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string? ImagenUrl { get; set; }
        public string VendedorId { get; set; } = string.Empty;
        public string? NombreVendedor { get; set; }
        public decimal Subtotal => Precio * Cantidad;
    }

    public class CarritoViewModel
    {
        public List<CarritoItemViewModel> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(i => i.Subtotal);
        public decimal CostoEnvioEstimado { get; set; } = 2500m;
        public decimal Total => Subtotal + CostoEnvioEstimado;
    }

    public class CheckoutViewModel
    {
        public List<CarritoItemViewModel> Items { get; set; } = new();

        [Required(ErrorMessage = "La dirección de entrega es obligatoria")]
        [Display(Name = "Dirección de Entrega (Calle y Altura)")]
        public string DireccionEntrega { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione la provincia")]
        [Display(Name = "Provincia")]
        public int ProvinciaId { get; set; }

        [Required(ErrorMessage = "Seleccione el nodo de distribución o localidad")]
        [Display(Name = "Nodo Logístico / Localidad")]
        public int NodoDistribucionId { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        [Display(Name = "Ciudad")]
        public string CiudadEntrega { get; set; } = string.Empty;

        [Display(Name = "Código Postal")]
        public string? CodigoPostal { get; set; }

        [Required(ErrorMessage = "El teléfono de contacto es obligatorio")]
        [Display(Name = "Teléfono de Contacto")]
        public string TelefonoContacto { get; set; } = string.Empty;

        [Display(Name = "Notas para el repartidor")]
        public string? NotasAdicionales { get; set; }

        [Required(ErrorMessage = "Seleccione un método de pago")]
        [Display(Name = "Método de Pago")]
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Transferencia;

        public decimal Subtotal { get; set; }
        public decimal CostoEnvio { get; set; }
        public decimal Total { get; set; }

        // Dropdowns anidados
        public IEnumerable<SelectListItem>? ProvinciasList { get; set; }
        public IEnumerable<SelectListItem>? NodosList { get; set; }
    }

    public class CambiarEstadoPedidoViewModel
    {
        public int PedidoId { get; set; }
        public string CodigoSeguimiento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione el nuevo estado")]
        [Display(Name = "Nuevo Estado")]
        public EstadoPedido NuevoEstado { get; set; }

        [Display(Name = "Observación o Motivo (Obligatorio si se Cancela)")]
        [StringLength(500)]
        public string? MotivoObservacion { get; set; }

        [Display(Name = "Ubicación Actual")]
        public string Ubicacion { get; set; } = "Centro Distribución LCP";
    }

    public class SeguimientoViewModel
    {
        public string? CodigoBusqueda { get; set; }
        public Pedido? Pedido { get; set; }
        public bool Encontrado { get; set; }
        public string? MensajeError { get; set; }
    }
}
