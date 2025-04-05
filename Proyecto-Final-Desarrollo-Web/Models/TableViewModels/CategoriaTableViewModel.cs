using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    public class CategoriaTableViewModel
    {
        public int ID_Categoría { get; set; }

        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Total Productos")]
        public int TotalProductos { get; set; }
    }

    public class CategoriaTableRequest
    {
        // Parámetros de la paginación
        public int Start { get; set; }
        public int Length { get; set; }

        // Parámetros de la búsqueda
        public string SearchValue { get; set; }

        // Parámetros de ordenamiento
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
    }

    public class CategoriaTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<CategoriaTableViewModel> data { get; set; }

        public CategoriaTableResponse()
        {
            data = new List<CategoriaTableViewModel>();
        }
    }

    // Este es el ViewModel para Laboratorios, esta aquí porque estaba cansado
    public class LaboratorioTableViewModel
    {
        public int ID_Laboratorio { get; set; }

        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "País de Origen")]
        public string pais_origen { get; set; }

        [Display(Name = "Total Medicamentos")]
        public int TotalMedicamentos { get; set; }
    }

    public class LaboratorioTableRequest
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
        public string Pais { get; set; }
    }

    public class LaboratorioTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<LaboratorioTableViewModel> data { get; set; }

        public LaboratorioTableResponse()
        {
            data = new List<LaboratorioTableViewModel>();
        }
    }
}
