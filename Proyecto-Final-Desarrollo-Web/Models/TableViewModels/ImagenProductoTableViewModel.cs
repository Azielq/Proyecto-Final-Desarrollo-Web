using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    public class ImagenProductoTableViewModel
    {
        public int ID_Imagen { get; set; }

        [Display(Name = "Producto")]
        public string NombreProducto { get; set; }

        [Display(Name = "URL")]
        public string URL { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Principal")]
        public bool EsPrincipal { get; set; }

        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; }

        [Display(Name = "Vista previa")]
        public string UrlMiniatura { get; set; }
    }

    public class ImagenProductoTableRequest
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
        public int? ProductoId { get; set; }
        public bool? SoloPrincipales { get; set; }
        public bool? Estado { get; set; }
    }

    public class ImagenProductoTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<ImagenProductoTableViewModel> data { get; set; }

        public ImagenProductoTableResponse()
        {
            data = new List<ImagenProductoTableViewModel>();
        }
    }
}