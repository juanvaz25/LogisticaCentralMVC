using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LCP.Web.Models.ViewModels.Productos
{
    public class ProductoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres")]
        [Display(Name = "Título de la Publicación")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(2000, ErrorMessage = "La descripción no puede superar los 2000 caracteres")]
        [Display(Name = "Descripción del Producto")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 100000000, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio ($)")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El stock disponible es obligatorio")]
        [Range(0, 100000, ErrorMessage = "El stock debe ser un valor no negativo")]
        [Display(Name = "Stock Disponible")]
        public int Stock { get; set; } = 1;

        [Display(Name = "Imagen Actual")]
        public string? ImagenUrl { get; set; }

        [Display(Name = "Subir Imagen")]
        public IFormFile? ImagenArchivo { get; set; }

        [StringLength(50)]
        [Display(Name = "Código SKU / Referencia")]
        public string? CodigoSku { get; set; }

        [Range(0.01, 1000, ErrorMessage = "El peso estimado debe ser mayor a 0")]
        [Display(Name = "Peso Estimado (kg)")]
        public decimal PesoKg { get; set; } = 1.0m;

        [Display(Name = "¿Publicación Activa?")]
        public bool Activo { get; set; } = true;

        [Display(Name = "¿Destacar Producto?")]
        public bool Destacado { get; set; } = false;

        [Required(ErrorMessage = "Debe seleccionar una categoría")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        // Para poblar select
        public IEnumerable<SelectListItem>? CategoriasList { get; set; }

        public string? VendedorId { get; set; }
        public string? NombreVendedor { get; set; }
        public string? NombreComercio { get; set; }
    }

    public class CatalogoFiltroViewModel
    {
        public string? Busqueda { get; set; }
        public int? CategoriaId { get; set; }
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }
        public string? OrdenarPor { get; set; } // "precio_asc", "precio_desc", "recientes", "nombre"
        public int Pagina { get; set; } = 1;
        public int ElementosPorPagina { get; set; } = 8;
        public int TotalElementos { get; set; }
        public int TotalPaginas => (int)Math.Ceiling((double)TotalElementos / ElementosPorPagina);

        public List<LCP.Web.Models.Entities.Producto> Productos { get; set; } = new();
        public List<LCP.Web.Models.Entities.Categoria> Categorias { get; set; } = new();
    }
}
