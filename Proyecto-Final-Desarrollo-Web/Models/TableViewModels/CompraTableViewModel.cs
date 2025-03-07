using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    public class CompraTableViewModel
    {
        public int id_compra { get; set; }

        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? fecha { get; set; }

        [Display(Name = "Proveedor")]
        public string NombreProveedor { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal total { get; set; }

        [Display(Name = "Estado")]
        public string estado { get; set; }

        [Display(Name = "Productos")]
        public int NumeroProductos { get; set; }

        // Propiedades calculadas
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

    public class CompraTableRequest
    {
        // Para la paginación
        public int Start { get; set; }
        public int Length { get; set; }

        // Para la búsqueda
        public string SearchValue { get; set; }

        // Para el ordenamiento
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }

        // Estos son unos filtros adicionales
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Estado { get; set; }
        public int? ProveedorId { get; set; }
    }

    public class CompraTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<CompraTableViewModel> data { get; set; }
        public decimal TotalMonto { get; set; }

        public CompraTableResponse()
        {
            data = new List<CompraTableViewModel>();
        }
    }

    public class DetalleCompraTableViewModel
    {
        public int id_detalle_compra { get; set; }
        public int ID_Compra { get; set; }

        [Display(Name = "Medicamento")]
        public string NombreMedicamento { get; set; }

        [Display(Name = "Precio Unitario")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PrecioUnitario { get; set; }

        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Display(Name = "Subtotal")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Subtotal { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        [Display(Name = "Laboratorio")]
        public string Laboratorio { get; set; }
    }
}