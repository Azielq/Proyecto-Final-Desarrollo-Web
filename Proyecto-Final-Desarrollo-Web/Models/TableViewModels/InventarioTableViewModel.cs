using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    public class InventarioTableViewModel
    {
        public int id_Inventario { get; set; }
        public int ID_Lote { get; set; }

        [Display(Name = "Producto")]
        public string NombreProducto { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        // Se ha eliminado la propiedad Laboratorio

        [Display(Name = "Lote")]
        public string NumeroLote { get; set; }

        [Display(Name = "Ubicación")]
        public string ubicacion { get; set; }

        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Display(Name = "Fecha Vencimiento")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaVencimiento { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; }
    }

    public class InventarioTableRequest
    {
        // Para la paginación
        public int Start { get; set; }
        public int Length { get; set; }

        // Para la búsqueda
        public string SearchValue { get; set; }

        // Para el ordenamiento
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }

        // Filtros adicionales  
        public string Ubicacion { get; set; }
        public int? ProductoId { get; set; } // Renombrado de MedicamentoId
        public int? CategoriaId { get; set; }
        public bool? SoloVencidos { get; set; }
        public bool? SoloPorVencer { get; set; }
    }

    public class InventarioTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<InventarioTableViewModel> data { get; set; }
        public decimal ValorTotal { get; set; }
        public int TotalUnidades { get; set; }

        public InventarioTableResponse()
        {
            data = new List<InventarioTableViewModel>();
        }
    }
}
