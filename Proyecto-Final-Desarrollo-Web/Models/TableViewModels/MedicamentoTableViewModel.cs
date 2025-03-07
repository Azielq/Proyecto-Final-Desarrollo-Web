using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    public class MedicamentoTableViewModel
    {
        public int ID_Medicamento { get; set; }

        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Principio Activo")]
        public string principio_activo { get; set; }

        [Display(Name = "Categoría")]
        public string NombreCategoria { get; set; }

        [Display(Name = "Laboratorio")]
        public string NombreLaboratorio { get; set; }

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

        // Estas son las propiedades para la tabla
        public bool StockBajo
        {
            get { return StockTotal < 10; }
        }

        public bool TieneLotesProximosAVencer { get; set; }
    }

    public class MedicamentoTableRequest
    {
        // Estos son Parámetros de paginación
        public int Start { get; set; }
        public int Length { get; set; }

        // Estos son Parámetros de búsqueda
        public string SearchValue { get; set; }

        // Estos son Parámetros de ordenamiento
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }

        // Estos son Filtros adicionaless
        public int? CategoriaId { get; set; }
        public int? LaboratorioId { get; set; }
        public string Estado { get; set; }
        public bool? StockBajo { get; set; }
    }

    public class MedicamentoTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<MedicamentoTableViewModel> data { get; set; }

        public MedicamentoTableResponse()
        {
            data = new List<MedicamentoTableViewModel>();
        }
    }
}