using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    public class FacturaTableViewModel
    {
        public int id_Factura { get; set; }

        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? fecha { get; set; }

        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Documento")]
        public string DocumentoCliente { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal total { get; set; }

        [Display(Name = "Estado")]
        public string estado { get; set; }

        [Display(Name = "Productos")]
        public int NumeroProductos { get; set; }

        // Propiedades calculadas
        public bool EsCancelable
        {
            get { return estado == "Completada"; }
        }

        public string BadgeClass
        {
            get
            {
                switch (estado)
                {
                    case "Completada":
                        return "bg-success";
                    case "Pendiente":
                        return "bg-warning";
                    case "Cancelada":
                        return "bg-danger";
                    default:
                        return "bg-secondary";
                }
            }
        }
    }

    public class FacturaTableRequest
    {
        // Para la paginación
        public int Start { get; set; }
        public int Length { get; set; }

        // Para la búsqueda
        public string SearchValue { get; set; }

        // Para el ordenamiento
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
        
        // Estos son filtros adicionales
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Estado { get; set; }
        public int? ClienteId { get; set; }
    }

    public class FacturaTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<FacturaTableViewModel> data { get; set; }
        public decimal TotalMonto { get; set; }

        public FacturaTableResponse()
        {
            data = new List<FacturaTableViewModel>();
        }
    }

    public class DetalleFacturaTableViewModel
    {
        public int ID_Detalle_Factura { get; set; }
        public int id_Factura { get; set; }

        [Display(Name = "Medicamento")]
        public string NombreMedicamento { get; set; }

        [Display(Name = "Precio Unitario")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PrecioUnitario { get; set; }

        [Display(Name = "Cantidad")]
        public int cantidad { get; set; }

        [Display(Name = "Subtotal")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal subtotal { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        [Display(Name = "Laboratorio")]
        public string Laboratorio { get; set; }
    }
}