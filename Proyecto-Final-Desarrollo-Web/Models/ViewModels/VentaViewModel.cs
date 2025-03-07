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

        // Información adicional de los cliente
        [Display(Name = "Nombre del Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Documento")]
        public string DocumentoCliente { get; set; }

        [Display(Name = "Teléfono")]
        public string TelefonoCliente { get; set; }

        // Esto es para la lista de detalles de la factura
        public List<DetalleVentaViewModel> Detalles { get; set; }

        public VentaViewModel()
        {
            Detalles = new List<DetalleVentaViewModel>();
            fecha = DateTime.Now;
            estado = "Pendiente";
        }

        // El metodillo convertir la entidad a ViewModel
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

        // Métodillo para convertir el ViewModel a entidad
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

        // Este método para calcular el total a partir de los detalles
        public void CalcularTotal()
        {
            this.total = Detalles?.Sum(d => d.subtotal) ?? 0;
        }
    }

    public class DetalleVentaViewModel
    {
        public int ID_Detalle_Factura { get; set; }
        public int id_Factura { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un medicamento")]
        [Display(Name = "Medicamento")]
        public int ID_Medicamento { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, 10000, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int cantidad { get; set; }

        [Display(Name = "Subtotal")]
        public decimal subtotal { get; set; }

        // Propiedades adicionales para la vista
        [Display(Name = "Nombre del Medicamento")]
        public string NombreMedicamento { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        // Métodillo para convertir la entidad a ViewModel
        public static DetalleVentaViewModel FromEntity(Detalles_Factura detalle)
        {
            return new DetalleVentaViewModel
            {
                ID_Detalle_Factura = detalle.ID_Detalle_Factura,
                id_Factura = detalle.id_Factura,
                ID_Medicamento = detalle.ID_Medicamento,
                cantidad = detalle.cantidad,
                subtotal = detalle.subtotal,
                NombreMedicamento = detalle.Medicamentos?.Nombre,
                Categoria = detalle.Medicamentos?.Categorias?.Nombre,
                PrecioUnitario = detalle.cantidad > 0 ? Math.Round(detalle.subtotal / detalle.cantidad, 2) : 0
            };
        }

        // Al revés, convierte el ViewModel a entidad
        public Detalles_Factura ToEntity()
        {
            return new Detalles_Factura
            {
                ID_Detalle_Factura = this.ID_Detalle_Factura,
                id_Factura = this.id_Factura,
                ID_Medicamento = this.ID_Medicamento,
                cantidad = this.cantidad,
                subtotal = this.subtotal
            };
        }
    }
}