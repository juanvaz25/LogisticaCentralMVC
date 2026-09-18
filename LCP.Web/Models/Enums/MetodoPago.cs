using System.ComponentModel.DataAnnotations;

namespace LCP.Web.Models.Enums
{
    public enum MetodoPago
    {
        [Display(Name = "Transferencia Bancaria")]
        Transferencia = 1,

        [Display(Name = "Tarjeta de Débito / Crédito")]
        Tarjeta = 2,

        [Display(Name = "Efectivo contra entrega")]
        EfectivoContraEntrega = 3,

        [Display(Name = "Billetera Virtual")]
        BilleteraVirtual = 4
    }
}
