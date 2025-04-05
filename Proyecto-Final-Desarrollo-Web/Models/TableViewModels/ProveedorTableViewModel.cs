using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    public class ProveedorTableViewModel
    {
        public int Pk_Proveedor { get; set; }

        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Correo")]
        public string Correo { get; set; }

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Display(Name = "Dirección")]
        public string direccion { get; set; }

        [Display(Name = "Estado")]
        public bool activo { get; set; }

        [Display(Name = "Total Compras")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalCompras { get; set; }

        [Display(Name = "Última Compra")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? UltimaCompra { get; set; }

        [Display(Name = "Compras Realizadas")]
        public int NumeroCompras { get; set; }

        // Propiedades calculadas
        public string EstadoTexto
        {
            get { return activo ? "Activo" : "Inactivo"; }
        }

        public string EstadoClass
        {
            get { return activo ? "badge bg-success" : "badge bg-danger"; }
        }
    }

    public class ProveedorTableRequest
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
        public bool? SoloActivos { get; set; }
    }

    public class ProveedorTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<ProveedorTableViewModel> data { get; set; }

        public ProveedorTableResponse()
        {
            data = new List<ProveedorTableViewModel>();
        }
    }
}