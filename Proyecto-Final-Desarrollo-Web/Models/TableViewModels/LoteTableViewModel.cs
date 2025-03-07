using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    public class LoteTableViewModel
    {
        public int id_Lote { get; set; }

        [Display(Name = "Medicamento")]
        public string NombreMedicamento { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        [Display(Name = "Laboratorio")]
        public string Laboratorio { get; set; }

        [Display(Name = "Cantidad")]
        public int cantidad { get; set; }

        [Display(Name = "Fecha Vencimiento")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime fecha_vencimiento { get; set; }

        [Display(Name = "Ubicación")]
        public string Ubicacion { get; set; }

        [Display(Name = "Días para Vencer")]
        public int DiasParaVencer { get; set; }

        // Propiedades calculadas
        [Display(Name = "Estado")]
        public string Estado
        {
            get
            {
                if (DiasParaVencer < 0)
                    return "Vencido";
                else if (DiasParaVencer <= 30)
                    return "Por Vencer";
                else
                    return "Vigente";
            }
        }

        public string EstadoClass
        {
            get
            {
                if (DiasParaVencer < 0)
                    return "text-danger";
                else if (DiasParaVencer <= 30)
                    return "text-warning";
                else
                    return "text-success";
            }
        }
    }

    public class LoteTableRequest
    {
        // Para la paginación
        public int Start { get; set; }
        public int Length { get; set; }

        // Para la búsqueda
        public string SearchValue { get; set; }

        // Para el ordenamiento
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }

        // Filtros que agregué adicionales
        public int? MedicamentoId { get; set; }
        public int? CategoriaId { get; set; }
        public bool? SoloVencidos { get; set; }
        public bool? SoloPorVencer { get; set; }
        public bool? SoloConStock { get; set; }
    }

    public class LoteTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<LoteTableViewModel> data { get; set; }
        public int TotalUnidades { get; set; }
        public int TotalLotes { get; set; }
        public int LotesVencidos { get; set; }
        public int LotesPorVencer { get; set; }

        public LoteTableResponse()
        {
            data = new List<LoteTableViewModel>();
        }
    }
}