using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    // ViewModel principal para el carrito
    public class CarritoViewModel
    {
        public int ID_Usuario { get; set; }
        public List<CarritoItemViewModel> Items { get; set; }

        [Display(Name = "Subtotal")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Subtotal => Items?.Sum(item => item.PrecioUnitario * item.Cantidad) ?? 0;

        [Display(Name = "Total Items")]
        public int TotalItems => Items?.Sum(item => item.Cantidad) ?? 0;

        public PagoViewModel InformacionPago { get; set; }

        public CarritoViewModel()
        {
            Items = new List<CarritoItemViewModel>();
        }
    }

    // ViewModel para cada item del carrito
    public class CarritoItemViewModel
    {
        public int ID_Carrito { get; set; }
        public int ID_Producto { get; set; }
        public int ID_Usuario { get; set; }

        [Display(Name = "Producto")]
        public string NombreProducto { get; set; }

        [Display(Name = "Imagen")]
        public string ImagenUrl { get; set; }

        [Display(Name = "Precio")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PrecioUnitario { get; set; }

        [Display(Name = "Cantidad")]
        [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100")]
        public int Cantidad { get; set; }

        [Display(Name = "Fecha")]
        public DateTime? FechaAgregado { get; set; }
    }

    // ViewModel para transacciones CRUD del carrito
    public class CarritoCrudViewModel
    {
        // Para agregar un producto al carrito
        public class AgregarAlCarritoViewModel
        {
            [Required]
            public int ID_Producto { get; set; }

            [Required(ErrorMessage = "La cantidad es requerida")]
            [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100")]
            public int Cantidad { get; set; } = 1;
        }

        // Para actualizar un item del carrito
        public class ActualizarCarritoViewModel
        {
            [Required]
            public int ID_Carrito { get; set; }

            [Required(ErrorMessage = "La cantidad es requerida")]
            [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100")]
            public int Cantidad { get; set; }
        }

        // Para eliminar un item del carrito
        public class EliminarDelCarritoViewModel
        {
            [Required]
            public int ID_Carrito { get; set; }
        }
    }

    // ViewModel para la información de pago
    public class PagoViewModel
    {
        [Display(Name = "Subtotal")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Subtotal { get; set; }

        [Display(Name = "Impuesto (IVA)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Impuesto { get; set; }

        [Display(Name = "Gastos de Envío")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal GastosEnvio { get; set; }

        [Display(Name = "Descuento")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Descuento { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Total => Subtotal + Impuesto + GastosEnvio - Descuento;
    }
}