using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    // ViewModel para un elemento individual del carrito
    public class CarritoItemViewModel
    {
        public int ID_Carrito { get; set; }
        public int ID_Producto { get; set; }
        public int ID_Usuario { get; set; }

        [Display(Name = "Nombre del Producto")]
        public string NombreProducto { get; set; }

        [Display(Name = "Imagen")]
        public string ImagenUrl { get; set; }

        [Display(Name = "Precio Unitario")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PrecioUnitario { get; set; }

        [Display(Name = "Cantidad")]
        [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100")]
        public int Cantidad { get; set; }

        [Display(Name = "Subtotal")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Subtotal => PrecioUnitario * Cantidad;

        [Display(Name = "Fecha Agregado")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? FechaAgregado { get; set; }
    }

    // ViewModel para la página del carrito completo
    public class CarritoViewModel
    {
        public List<CarritoItemViewModel> Items { get; set; } = new List<CarritoItemViewModel>();

        [Display(Name = "Subtotal")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Subtotal => Items.Sum(item => item.Subtotal);

        [Display(Name = "Cantidad de Productos")]
        public int TotalItems => Items.Sum(item => item.Cantidad);

        public int ID_Usuario { get; set; }

        // Esta propiedad se usará para la vista de pago
        public PagoViewModel InformacionPago { get; set; }
    }

    // ViewModel para operaciones CRUD del carrito
    public class CarritoCrudViewModel
    {
        // Para agregar un producto al carrito
        public class AgregarAlCarritoViewModel
        {
            public int ID_Producto { get; set; }

            [Required(ErrorMessage = "La cantidad es requerida")]
            [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100")]
            public int Cantidad { get; set; } = 1;
        }

        // Para actualizar la cantidad de un producto en el carrito
        public class ActualizarCarritoViewModel
        {
            public int ID_Carrito { get; set; }

            [Required(ErrorMessage = "La cantidad es requerida")]
            [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100")]
            public int Cantidad { get; set; }
        }

        // Para eliminar un producto del carrito
        public class EliminarDelCarritoViewModel
        {
            public int ID_Carrito { get; set; }
        }
    }

    // ViewModel para la página de pago
    public class PagoViewModel
    {
        [Display(Name = "Subtotal")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Subtotal { get; set; }

        [Display(Name = "IVA")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Impuesto { get; set; }

        [Display(Name = "Gastos de Envío")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal GastosEnvio { get; set; }

        [Display(Name = "Descuento")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Descuento { get; set; }

        [Display(Name = "Total a Pagar")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Total => Subtotal + Impuesto + GastosEnvio - Descuento;
    }
}