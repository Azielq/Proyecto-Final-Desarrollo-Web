using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class VentaViewModel
    {
        // Datos de la factura
        public int id_Factura { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        [Display(Name = "Cliente")]
        public int ID_Cliente { get; set; }

        [Display(Name = "Fecha")]
        public DateTime? fecha { get; set; }

        [Display(Name = "Total")]
        public decimal total { get; set; }

        [Display(Name = "Estado")]
        public string estado { get; set; }

        // Información adicional del cliente
        [Display(Name = "Nombre del Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Documento")]
        public string DocumentoCliente { get; set; }

        [Display(Name = "Teléfono")]
        public string TelefonoCliente { get; set; }

        // Lista de detalles de la factura
        public List<DetalleVentaViewModel> Detalles { get; set; }

        public VentaViewModel()
        {
            Detalles = new List<DetalleVentaViewModel>();
            fecha = DateTime.Now;
            estado = "Pendiente";
        }

        // Método para convertir la entidad a ViewModel
        public static VentaViewModel FromEntity(Facturas factura)
        {
            var viewModel = new VentaViewModel
            {
                id_Factura = factura.id_Factura,
                ID_Cliente = factura.ID_Cliente,
                fecha = factura.fecha,
                total = factura.total,
                estado = factura.estado
            };

            if (factura.Clientes?.Personas != null)
            {
                var persona = factura.Clientes.Personas;
                viewModel.NombreCliente = $"{persona.Nombre} {persona.Apellido_1} {persona.Apellido_2}";
                viewModel.DocumentoCliente = $"{persona.tipo_documento} {persona.numero_documento}";
                viewModel.TelefonoCliente = persona.Teléfono;
            }

            if (factura.Detalles_Factura != null)
            {
                viewModel.Detalles = factura.Detalles_Factura
                    .Select(d => DetalleVentaViewModel.FromEntity(d))
                    .ToList();
            }

            return viewModel;
        }

        // Método para convertir el ViewModel a entidad
        public Facturas ToEntity()
        {
            var factura = new Facturas
            {
                id_Factura = this.id_Factura,
                ID_Cliente = this.ID_Cliente,
                fecha = this.fecha ?? DateTime.Now,
                total = this.total,
                estado = this.estado ?? "Pendiente"
            };

            return factura;
        }

        // Método para calcular el total a partir de los detalles
        public void CalcularTotal()
        {
            this.total = Detalles?.Sum(d => d.subtotal) ?? 0;
        }
    }

    public class DetalleVentaViewModel
    {
        public int ID_Detalle_Factura { get; set; }
        public int id_Factura { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un producto")]
        [Display(Name = "Producto")]
        public int ID_Producto { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, 10000, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int cantidad { get; set; }

        [Display(Name = "Subtotal")]
        public decimal subtotal { get; set; }

        // Propiedades adicionales para la vista
        [Display(Name = "Nombre del Producto")]
        public string NombreProducto { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario
        {
            get { return cantidad > 0 ? Math.Round(subtotal / cantidad, 2) : 0; }
            set { /* Setter requerido para la propiedad */ }
        }

        // Método para convertir la entidad a ViewModel
        public static DetalleVentaViewModel FromEntity(Detalles_Factura detalle)
        {
            return new DetalleVentaViewModel
            {
                ID_Detalle_Factura = detalle.ID_Detalle_Factura,
                id_Factura = detalle.id_Factura,
                // Se asume que en la entidad de Detalles_Factura se actualizó a ID_Producto
                ID_Producto = detalle.ID_Producto,
                cantidad = detalle.cantidad,
                subtotal = detalle.subtotal,
                // Se asume que la relación se renombró de Medicamentos a Productos
                NombreProducto = detalle.Productos?.Nombre,
                Categoria = detalle.Productos?.Categorias?.Nombre,
                PrecioUnitario = detalle.cantidad > 0 ? Math.Round(detalle.subtotal / detalle.cantidad, 2) : 0
            };
        }

        // Método para convertir el ViewModel a entidad
        public Detalles_Factura ToEntity()
        {
            return new Detalles_Factura
            {
                ID_Detalle_Factura = this.ID_Detalle_Factura,
                id_Factura = this.id_Factura,
                // Se mapea la propiedad nueva
                ID_Producto = this.ID_Producto,
                cantidad = this.cantidad,
                subtotal = this.subtotal
            };
        }
    }
}
