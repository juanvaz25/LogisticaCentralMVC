using System.ComponentModel.DataAnnotations;

namespace LCP.Web.Models.Enums
{
    public enum EstadoPedido
    {
        [Display(Name = "En proceso")]
        EnProceso = 1,

        [Display(Name = "En preparación")]
        EnPreparacion = 2,

        [Display(Name = "En camino")]
        EnCamino = 3,

        [Display(Name = "Entregado")]
        Entregado = 4,

        [Display(Name = "Cancelado")]
        Cancelado = 5
    }
}
