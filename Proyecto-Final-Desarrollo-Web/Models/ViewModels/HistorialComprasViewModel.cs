using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class HistorialComprasViewModel
    {
        public List<FacturaUsuarioViewModel> Facturas { get; set; }
        public int TotalFacturas { get; set; }
        public decimal TotalGastado { get; set; }

        public HistorialComprasViewModel()
        {
            Facturas = new List<FacturaUsuarioViewModel>();
        }
    }

    public class FacturaUsuarioViewModel
    {
        public int Id_Factura { get; set; }

        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime Fecha { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Total { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; }

        [Display(Name = "Productos")]
        public int CantidadProductos { get; set; }

        public List<DetalleFacturaSimpleViewModel> Detalles { get; set; }

        public FacturaUsuarioViewModel()
        {
            Detalles = new List<DetalleFacturaSimpleViewModel>();
        }

        public string EstadoClaseBadge
        {
            get
            {
                switch (Estado?.ToLower())
                {
                    case "pagado":
                        return "bg-success";
                    case "pendiente":
                        return "bg-warning";
                    case "cancelada":
                        return "bg-danger";
                    default:
                        return "bg-secondary";
                }
            }
        }
    }

    public class DetalleFacturaSimpleViewModel
    {
        public int ID_Producto { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PrecioUnitario { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Subtotal { get; set; }
    }
}