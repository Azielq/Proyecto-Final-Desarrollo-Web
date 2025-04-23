using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class FacturaViewModel
    {
        public int id_Factura { get; set; }

        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? fecha { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio")]
        [Display(Name = "Cliente")]
        public int ID_Cliente { get; set; }

        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Documento")]
        public string DocumentoCliente { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal total { get; set; }

        [Display(Name = "Estado")]
        public string estado { get; set; }

        [Display(Name = "Método de Pago")]
        public string MetodoPagoSeleccionado { get; set; }

        [Display(Name = "Notas")]
        [DataType(DataType.MultilineText)]
        public string notas { get; set; }

        // Lista de productos para la factura
        public List<DetalleFacturaViewModel> Detalles { get; set; }

        // Lista de productos en el carrito
        public List<ProductoSeleccionadoViewModel> ProductosSeleccionados { get; set; }

        // Para dropdown de métodos de pago
        public IEnumerable<SelectListItem> MetodosPagoList
        {
            get
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Value = "Efectivo", Text = "Efectivo" },
                    new SelectListItem { Value = "Tarjeta de Crédito", Text = "Tarjeta de Crédito" },
                    new SelectListItem { Value = "Tarjeta de Débito", Text = "Tarjeta de Débito" },
                    new SelectListItem { Value = "Transferencia", Text = "Transferencia Bancaria" },
                    new SelectListItem { Value = "Otro", Text = "Otro" }
                };
            }
        }

        // Para dropdown de clientes
        public IEnumerable<SelectListItem> ClientesList { get; set; }

        public FacturaViewModel()
        {
            Detalles = new List<DetalleFacturaViewModel>();
            ProductosSeleccionados = new List<ProductoSeleccionadoViewModel>();
            fecha = DateTime.Now;
            estado = "Pendiente";
        }

        // Método para convertir la entidad a ViewModel
        public static FacturaViewModel FromEntity(Facturas factura)
        {
            var viewModel = new FacturaViewModel
            {
                id_Factura = factura.id_Factura,
                fecha = factura.fecha,
                ID_Cliente = factura.ID_Cliente,
                total = factura.total,
                estado = factura.estado
            };

            // Cargar datos del cliente si existe
            if (factura.Clientes != null && factura.Clientes.Personas != null)
            {
                var cliente = factura.Clientes;
                var persona = cliente.Personas;
                viewModel.NombreCliente = $"{persona.Nombre} {persona.Apellido_1} {persona.Apellido_2}".Trim();
                viewModel.DocumentoCliente = $"{persona.tipo_documento}: {persona.numero_documento}";
            }

            // Cargar detalles de la factura
            if (factura.Detalles_Factura != null)
            {
                viewModel.Detalles = factura.Detalles_Factura
                    .Select(d => DetalleFacturaViewModel.FromEntity(d))
                    .ToList();
            }

            return viewModel;
        }

        // Método para convertir ViewModel a Entidad
        public Facturas ToEntity()
        {
            return new Facturas
            {
                id_Factura = this.id_Factura,
                fecha = this.fecha ?? DateTime.Now,
                ID_Cliente = this.ID_Cliente,
                total = this.total,
                estado = this.estado ?? "Pendiente"
            };
        }
    }

    public class DetalleFacturaViewModel
    {
        public int ID_Detalle_Factura { get; set; }
        public int id_Factura { get; set; }

        [Required(ErrorMessage = "El producto es obligatorio")]
        [Display(Name = "Producto")]
        public int ID_Producto { get; set; }

        [Display(Name = "Producto")]
        public string NombreProducto { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int cantidad { get; set; }

        [Display(Name = "Precio")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PrecioUnitario { get; set; }

        [Display(Name = "Subtotal")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal subtotal { get; set; }

        [Display(Name = "Imagen")]
        public string UrlImagen { get; set; }

        [Display(Name = "Stock Disponible")]
        public int StockDisponible { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        // Método para convertir la entidad a ViewModel
        public static DetalleFacturaViewModel FromEntity(Detalles_Factura detalle)
        {
            var viewModel = new DetalleFacturaViewModel
            {
                ID_Detalle_Factura = detalle.ID_Detalle_Factura,
                id_Factura = detalle.id_Factura,
                ID_Producto = detalle.ID_Producto,
                cantidad = detalle.cantidad,
                subtotal = detalle.subtotal
            };

            // Cargar datos del producto si existe
            if (detalle.Productos != null)
            {
                var producto = detalle.Productos;
                viewModel.NombreProducto = producto.Nombre;
                viewModel.Categoria = producto.Categorias?.Nombre;
                viewModel.PrecioUnitario = detalle.subtotal / detalle.cantidad; // Calcular precio unitario

                // Obtener stock disponible
                viewModel.StockDisponible = producto.Lotes != null ?
                    producto.Lotes.Sum(l => l.cantidad) : 0;

                // Obtener URL de la imagen principal si existe
                var imagenPrincipal = producto.Imagenes_Producto?.FirstOrDefault(i => i.Estado && i.EsPrincipal);
                viewModel.UrlImagen = imagenPrincipal?.URL;
            }

            return viewModel;
        }

        // Método para convertir ViewModel a Entidad
        public Detalles_Factura ToEntity()
        {
            return new Detalles_Factura
            {
                ID_Detalle_Factura = this.ID_Detalle_Factura,
                id_Factura = this.id_Factura,
                ID_Producto = this.ID_Producto,
                cantidad = this.cantidad,
                subtotal = this.subtotal
            };
        }
    }

    public class ProductoSeleccionadoViewModel
    {
        public int ID_Producto { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string UrlImagen { get; set; }
        public decimal Subtotal { get { return PrecioUnitario * Cantidad; } }
        public int StockDisponible { get; set; }
        public string Categoria { get; set; }
    }

    public class POSViewModel
    {
        public FacturaViewModel Factura { get; set; }
        public List<ProductoViewModel> ProductosDisponibles { get; set; }
        public List<ProductoSeleccionadoViewModel> ProductosSeleccionados { get; set; }

        [Display(Name = "Cliente")]
        public int ID_Cliente { get; set; }

        [Display(Name = "Método de Pago")]
        public string MetodoPago { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Total { get; set; }

        public POSViewModel()
        {
            Factura = new FacturaViewModel();
            ProductosDisponibles = new List<ProductoViewModel>();
            ProductosSeleccionados = new List<ProductoSeleccionadoViewModel>();
        }
    }

    // Se eliminó la clase FacturaResumenViewModel para evitar ambigüedades con la existente
}

// Esta clase está en un archivo separado para evitar conflictos de ambigüedad
namespace Proyecto_Final_Desarrollo_Web.ViewModels.Facturacion
{
    public class MiFacturaResumenViewModel
    {
        [Display(Name = "Número")]
        public int id_Factura { get; set; }

        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? fecha { get; set; }

        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal total { get; set; }

        [Display(Name = "Estado")]
        public string estado { get; set; }

        [Display(Name = "Productos")]
        public int CantidadProductos { get; set; }

        // Estado de color para la insignia
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
}