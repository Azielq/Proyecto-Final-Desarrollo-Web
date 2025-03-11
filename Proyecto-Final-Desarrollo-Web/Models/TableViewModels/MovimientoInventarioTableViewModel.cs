using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    public class MovimientoInventarioTableViewModel
    {
        public int ID_movimiento { get; set; }

        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? fecha { get; set; }

        [Display(Name = "Medicamento")]
        public string NombreMedicamento { get; set; }

        [Display(Name = "Lote")]
        public int id_Lote { get; set; }

        [Display(Name = "Vencimiento")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaVencimiento { get; set; }

        [Display(Name = "Tipo")]
        public string tipo { get; set; }

        [Display(Name = "Cantidad")]
        public int cantidad { get; set; }

        [Display(Name = "Documento")]
        public string DocumentoReferencia { get; set; }

        [Display(Name = "Valor")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal ValorMovimiento { get; set; }

        // Propiedades calculadas
        public string TipoClass
        {
            get
            {
                switch (tipo)
                {
                    case "Entrada":
                    case "Entrada por Cancelación":
                        return "text-success";
                    case "Salida":
                    case "Ajuste Salida":
                        return "text-danger";
                    case "Ajuste Entrada":
                        return "text-primary";
                    default:
                        return "text-secondary";
                }
            }
        }

        public bool EsEntrada
        {
            get
            {
                return tipo.Contains("Entrada");
            }
        }
    }

    public class MovimientoInventarioTableRequest
    {
        // Para la paginación
        public int Start { get; set; }
        public int Length { get; set; }

        // Para la búsqueda
        public string SearchValue { get; set; }

        // Para lo del ordenamiento
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }

        // Estos son filtros adicionales
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? MedicamentoId { get; set; }
        public int? LoteId { get; set; }
        public string Tipo { get; set; }
    }

    public class MovimientoInventarioTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<MovimientoInventarioTableViewModel> data { get; set; }
        public int TotalEntradas { get; set; }
        public int TotalSalidas { get; set; }
        public decimal ValorTotalEntradas { get; set; }
        public decimal ValorTotalSalidas { get; set; }

        public MovimientoInventarioTableResponse()
        {
            data = new List<MovimientoInventarioTableViewModel>();
        }
    }
}