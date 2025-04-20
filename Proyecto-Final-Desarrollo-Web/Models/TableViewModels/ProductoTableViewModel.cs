using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    public class ProductoTableViewModel
    {
        public int ID_Producto { get; set; }

        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Categoría")]
        public string NombreCategoria { get; set; }

        [Display(Name = "Precio Compra")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal precio_compra { get; set; }

        [Display(Name = "Precio Venta")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal precio_venta { get; set; }

        [Display(Name = "Stock")]
        public int StockTotal { get; set; }

        [Display(Name = "Estado")]
        public string estado { get; set; }

        [Display(Name = "Margen")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public decimal Margen
        {
            get
            {
                if (precio_compra == 0) return 0;
                return (precio_venta - precio_compra) / precio_compra;
            }
        }

        // Propiedad para la imagen principal
        [Display(Name = "Imagen")]
        public string UrlImagenPrincipal { get; set; }

        // Propiedades adicionales para la tabla
        public bool StockBajo
        {
            get { return StockTotal < 10; }
        }

        public bool TieneLotesProximosAVencer { get; set; }

        // Cantidad de imágenes asociadas
        [Display(Name = "Imágenes")]
        public int CantidadImagenes { get; set; }
    }

    public class ProductoTableRequest
    {
        // Parámetros de paginación
        public int Start { get; set; }
        public int Length { get; set; }

        // Parámetros de búsqueda
        public string SearchValue { get; set; }

        // Parámetros de ordenamiento
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }

        // Filtros adicionales
        public int? CategoriaId { get; set; }
        // Se elimina LaboratorioId
        public string Estado { get; set; }
        public bool? StockBajo { get; set; }
    }

    public class ProductoTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<ProductoTableViewModel> data { get; set; }

        public ProductoTableResponse()
        {
            data = new List<ProductoTableViewModel>();
        }
    }
}
    